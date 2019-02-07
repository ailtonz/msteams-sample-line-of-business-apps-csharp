﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
using CrossVertical.Announcement.Models;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CrossVertical.Announcement.Helpers
{
    public class ProactiveMessageHelper
    {
        public static async Task<NotificationSendStatus> SendNotification(string serviceUrl, string tenantId, string userId, string messageText, Attachment attachment)
        {
            var connectorClient = new ConnectorClient(new Uri(serviceUrl));

            var parameters = new ConversationParameters
            {
                Members = new ChannelAccount[] { new ChannelAccount(userId) },
                ChannelData = new TeamsChannelData
                {
                    Tenant = new TenantInfo(tenantId),
                    Notification = new NotificationInfo() { Alert = true }
                }
            };

            try
            {
                var conversationResource = await connectorClient.Conversations.CreateConversationAsync(parameters);
                var replyMessage = Activity.CreateMessageActivity();
                replyMessage.Conversation = new ConversationAccount(id: conversationResource.Id.ToString());
                replyMessage.ChannelData = new TeamsChannelData() { Notification = new NotificationInfo(true) };
                replyMessage.Text = messageText;
                if (attachment != null)
                    replyMessage.Attachments.Add(attachment);

                var resourceResponse = await connectorClient.Conversations.SendToConversationAsync(conversationResource.Id, (Activity)replyMessage);
                return new NotificationSendStatus() { MessageId = resourceResponse.Id, IsSuccessful = true };

            }
            catch (Exception ex)
            {
                // Handle the error.
                ErrorLogService.LogError(ex);
                return new NotificationSendStatus() { IsSuccessful = false, FailureMessage = ex.Message };
            }
        }

        public static async Task<NotificationSendStatus> SendChannelNotification(ChannelAccount botAccount, string serviceUrl, string channelId, string messageText, Attachment attachment)
        {
            try
            {
                var replyMessage = Activity.CreateMessageActivity();
                replyMessage.Text = messageText;

                if (attachment != null)
                    replyMessage.Attachments.Add(attachment);

                var connectorClient = new ConnectorClient(new Uri(serviceUrl));

                var parameters = new ConversationParameters
                {
                    Bot = botAccount,
                    ChannelData = new TeamsChannelData
                    {
                        Channel = new ChannelInfo(channelId),
                        Notification = new NotificationInfo() { Alert = true }
                    },
                    IsGroup = true,
                    Activity = (Activity)replyMessage
                };

                var conversationResource = await connectorClient.Conversations.CreateConversationAsync(parameters);
                return new NotificationSendStatus() { MessageId = conversationResource.Id, IsSuccessful = true };
            }
            catch (Exception ex)
            {
                ErrorLogService.LogError(ex);
                return new NotificationSendStatus() { IsSuccessful = false, FailureMessage = ex.Message };
            }
        }
    }
}