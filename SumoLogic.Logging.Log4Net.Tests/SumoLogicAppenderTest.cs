/**
 *    _____ _____ _____ _____    __    _____ _____ _____ _____
 *   |   __|  |  |     |     |  |  |  |     |   __|     |     |
 *   |__   |  |  | | | |  |  |  |  |__|  |  |  |  |-   -|   --|
 *   |_____|_____|_|_|_|_____|  |_____|_____|_____|_____|_____|
 *
 *                UNICORNS AT WARP SPEED SINCE 2010
 *
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
namespace SumoLogic.Logging.Log4Net.Tests
{
    using System;
    using System.Net.Http;
    using log4net;
    using log4net.Core;
    using log4net.Layout;
    using log4net.Repository.Hierarchy;
    using SumoLogic.Logging.Common.Sender;
    using Xunit;

    /// <summary>
    /// <see cref="SumoLogicAppender"/> class related tests.
    /// </summary>
    public class SumoLogicAppenderTest : IDisposable
    {
        /// <summary>
        /// The HTTP messages handler mock.
        /// </summary>
        private MockHttpMessageHandler messagesHandler;

        /// <summary>
        /// The log4net log.
        /// </summary>
        private ILog log4netLog;

        /// <summary>
        /// The log4net logger.
        /// </summary>
        private Logger log4netLogger;

        /// <summary>
        /// The SumoLogic appender.
        /// </summary>
        private SumoLogicAppender sumoLogicAppender;

        /// <summary>
        /// Initializes a new instance of the <see cref="SumoLogicAppenderTest"/> class.
        /// </summary>
        public SumoLogicAppenderTest()
        {
            this.messagesHandler = new MockHttpMessageHandler();

            this.sumoLogicAppender = new SumoLogicAppender(null, this.messagesHandler);
            this.sumoLogicAppender.Url = "http://www.fakeadress.com";
            this.sumoLogicAppender.SourceName = "SumoLogicAppenderSourceName";
            this.sumoLogicAppender.SourceCategory = "SumoLogicAppenderSourceCategory";
            this.sumoLogicAppender.SourceHost = "SumoLogicAppenderSourceHost";
            this.sumoLogicAppender.Layout = new PatternLayout("-- %m%n");
            this.sumoLogicAppender.ActivateOptions();

            this.log4netLog = LogManager.GetLogger(typeof(SumoLogicAppenderTest));
            this.log4netLogger = this.log4netLog.Logger as Logger;
            this.log4netLogger.Additivity = false;
            this.log4netLogger.Level = Level.All;
            this.log4netLogger.RemoveAllAppenders();
            this.log4netLogger.AddAppender(this.sumoLogicAppender);
            this.log4netLogger.Repository.Configured = true;
        }

        /// <summary>
        /// Test logging of a single message.
        /// </summary>
        [Fact]
        public void TestSingleMessage()
        {
            this.log4netLog.Info("This is a message");

            Assert.Equal(1, this.messagesHandler.ReceivedRequests.Count);
            Assert.Equal("-- This is a message\r\n\r\n", this.messagesHandler.LastReceivedRequest.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// Test logging of multiple messages.
        /// </summary>
        [Fact]
        public void TestMultipleMessages()
        {
            int numMessages = 20;
            for (int i = 0; i < numMessages / 2; i++)
            {
                this.log4netLog.Info("info " + i);
                this.log4netLog.Error("error " + i);
            }

            Assert.Equal(numMessages, this.messagesHandler.ReceivedRequests.Count);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.log4netLogger.RemoveAllAppenders();
                this.messagesHandler.Dispose();
            }
        }
    }
}