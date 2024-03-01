using B360.Common.EntityObjects.Notifier.ExternalNotifications;
using B360.Notifier.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SampleChannel
{
    public class SampleNotificationChannel : IChannelNotification<SampleChannelSettings>
    {
        public string GetGlobalPropertiesSchema()
        {
            return Helper.GetResourceFileContent("GlobalProperties.json");
        }

        public string GetAlarmPropertiesSchema()
        {
            return Helper.GetResourceFileContent("AlarmProperties.json");
        }

        public SampleChannelSettings GetSettings(List<CustomNotificationChannel> channelSettings)
        {
            var sampleChannelSettings = new SampleChannelSettings();

            // Read and assign the channel global settings based on your need.

            return sampleChannelSettings;
        }

        // Method to Send Alarm Notification 
        public bool SendNotification(BizTalkEnvironment environment, Alarm alarm, string globalProperties, Dictionary<MonitorGroupTypeName, MonitorGroupData> notifications)
        {
            try
            {
                // Get Global Settings
                var channelSettings = JsonConvert.DeserializeObject<List<CustomNotificationChannel>>(globalProperties);
                var sampleChannelSettings = GetSettings(channelSettings);

                #region AlarmProperties

                // Get Alarm Settings
                if (alarm.AlarmProperties != null)
                {
                    var alarmProperties = JsonConvert.DeserializeObject<List<CustomNotificationChannelSettings>>(alarm.AlarmProperties);
                    if (alarmProperties != null)
                    {
                        // Read and assign the channel alarm settings based on your need.
                    }
                }

                #endregion

                //Construct Message
                string message = string.Empty;
                message += String.Format("\nAlarm Name: {0} \n\nAlarm Desc: {1} \n", alarm.Name, alarm.Description);
                message += "\n----------------------------------------------------------------------------------------------------\n";
                message += String.Format("\nEnvironment Name: {0} \n\nMgmt Sql Instance Name: {1} \nMgmt Sql Db Name: {2}\n", environment.Name, environment.MgmtSqlDbName, environment.MgmtSqlInstanceName);
                message += "\n----------------------------------------------------------------------------------------------------\n";

                // Change the Message Type and Format based on your need.
                BT360Helper helper = new BT360Helper(notifications, environment, alarm, MessageType.ConsolidatedMessage, MessageFormat.Text);
                message += helper.GetNotificationMessage();

                return SendMessageToNotificationChannel();

            }
            catch (Exception ex)
            {
                LoggingHelper.Info("Custom channel notification failed. Error " + ex.Message);
                return false;
            }
        }

        // Method to Send Analytics Dashboard and Secure SQL Query Report.
        public bool SendAnalyticsReport(BizTalkEnvironment environment, ReportingScheduler scheduler, string globalProperties, string getAnalyticsReportInStreamApi, CustomSQLQuery customSQLQuery = null)
        {
            try
            {
                // Note: Customise the following sample message template based on your need.

                #region Sample Message Template. 

                string message = string.Empty;
                if (scheduler.ReportType == (int)ReportType.dashboard)
                {
                    message += ($"\n Report Name: {scheduler.Name} \n\n Report Dashboard: {scheduler.ResourceName} \n");
                    message += "\n----------------------------------------------------------------------------------------------------\n";
                    message += ($"\n Environment Name: {environment.Name} \n\n Mgmt SQL Instance Name: {Regex.Escape(environment.MgmtSqlInstanceName)} \n\n Mgmt SQL Db Name: {environment.MgmtSqlDbName} \n");
                    message += "\n----------------------------------------------------------------------------------------------------\n";


                    dynamic serializedConfiguredWidgets = JsonConvert.DeserializeObject(scheduler.ConfiguredWidgets);

                    if (serializedConfiguredWidgets.Count > 0)
                    {
                        message += "\n Configured Widgets \n";
                        foreach (var configuredObject in serializedConfiguredWidgets)
                        {
                            message += $"\n - {configuredObject.title} \r";
                        }
                        message += "\n----------------------------------------------------------------------------------------------------\n";
                        message += $"\n Check BizTalk360 PDF Report here: {getAnalyticsReportInStreamApi}";

                        // Use the below property to send the report as an attachment instead of a link
                        // scheduler.FileStream - File Stream of the report 
                    }
                }
                else
                {
                    message += ($"\n Report Name: {scheduler.Name} \n\n");
                    message += "\n----------------------------------------------------------------------------------------------------\n";
                    message += ($"\n Environment Name: {environment.Name} \n\n Mgmt SQL Instance Name: {Regex.Escape(environment.MgmtSqlInstanceName)} \n\n Mgmt SQL Db Name: {environment.MgmtSqlDbName} \n");
                    message += "\n----------------------------------------------------------------------------------------------------\n";

                    message += $"\n Secure SQL Query: {scheduler.CustomSQLQuery.Name}  \n\n";
                    message += $"\n SQL Instance: {scheduler.CustomSQLQuery.SqlInstance}  \n\n";
                    message += $"\n Database: {scheduler.CustomSQLQuery.Database}  \n\n";
                    message += $"\n No of Records: {scheduler.CustomSQLQuery.SQLQueryRecordCount}  \n\n";
                    message += "\n----------------------------------------------------------------------------------------------------\n";
                    message += $"\n Check Secure SQL Query Excel Report here: {getAnalyticsReportInStreamApi}";

                    // Use the below property to send the report as an attachment instead of a link
                    // scheduler.FileStream - File Stream of the report 
                }

                #endregion

                return SendMessageToNotificationChannel();
            }
            catch (Exception ex)
            {
                LoggingHelper.Info("Custom channel notification failed. Error " + ex.Message);
                return false;
            }

        }

        // Method to Send Automated Tasks Result.
        public bool SendAutomatedTaskSummary(BizTalkEnvironment environment, AutomatedTaskInstanceDetail automatedTaskInstanceDetail, string globalProperties)
        {
            try
            {
                // Note: Customise the following sample message template based on your need.

                #region Sample Message Template. 

                string message = string.Empty;

                message += ($"\n Automated Task: {automatedTaskInstanceDetail.taskName}");
                message += !string.IsNullOrEmpty(automatedTaskInstanceDetail.description) ? ($"\n\n {automatedTaskInstanceDetail.description} \n") : ($"\n\n");
                message += "\n----------------------------------------------------------------------------------------------------\n";
                message += ($"\n Environment Name: {environment.Name} \n\n Mgmt SQL Instance Name: {Regex.Escape(environment.MgmtSqlInstanceName)} \n\n Mgmt SQL Db Name: {environment.MgmtSqlDbName} \n");
                message += "\n----------------------------------------------------------------------------------------------------\n";
                message += ($"\n Automated Task Execution Summary \n");

                if (automatedTaskInstanceDetail.configurationType != ConfigurationType.CustomWorkflow)
                {
                    List<GroupedResources> deserializedGroupedResources = JsonConvert.DeserializeObject<List<GroupedResources>>(automatedTaskInstanceDetail.actionExecutionDetails.FirstOrDefault().groupedResourcesList);
                    switch (automatedTaskInstanceDetail.configurationType)
                    {
                        case ConfigurationType.Application:
                        case ConfigurationType.PowerShellScript:
                            message += ($"\n Resource Type: {automatedTaskInstanceDetail.configurationType} " +
                            $"\n\n Resources Name: {automatedTaskInstanceDetail.actionExecutionDetails.FirstOrDefault().resourcesName} ");
                            break;
                        case ConfigurationType.LogicApps:
                            message += ($"\n Resource Type: {automatedTaskInstanceDetail.actionExecutionDetails.FirstOrDefault().resourceType} "
                              + $"\n\n Resources Detail: ");

                            foreach (GroupedResources group in deserializedGroupedResources)
                            {
                                if (group.groupCategory != AutomatedTaskConstants.ResubmittedRuns)
                                {
                                    message += $"\n {group.groupName} \n";
                                    message += $" {string.Join(", ", group.groupMembers)} ";
                                    message += $"\n\n";
                                }
                            }
                            break;
                        default:
                            message += ($"\n Resource Type: {automatedTaskInstanceDetail.actionExecutionDetails.FirstOrDefault().resourceType} "
                              + $"\n\n Resources Detail: ");
                            foreach (GroupedResources group in deserializedGroupedResources)
                            {
                                message += $"\n {group.groupName} \n";
                                message += $" {string.Join(", ", group.groupMembers)} ";
                                message += $"\n\n";
                            }
                            break;
                    }

                    message += $"\n\n Action: {automatedTaskInstanceDetail.actionExecutionDetails.FirstOrDefault().actionPerformed} ";

                    if (automatedTaskInstanceDetail.actionExecutionDetails.FirstOrDefault().actionPerformed == AutomatedTaskConstants.ResubmitAction)
                    {
                        message += $"\n Resubmitted Runs: \n";
                        foreach (GroupedResources group in deserializedGroupedResources)
                        {
                            if (group.groupCategory == AutomatedTaskConstants.ResubmittedRuns)
                            {
                                message += $"\n {group.groupName} \n";
                                message += $" {string.Join(", ", group.groupMembers)} ";
                                message += $"\n\n";
                            }
                        }
                    }
                    message += $"\n\n Status: {automatedTaskInstanceDetail.taskStatus} " +
                      $"\n\n Execution Type: {automatedTaskInstanceDetail.executionType} " +
                      $"\n\n Started At: {automatedTaskInstanceDetail.startedAt} " +
                      $"\n\n Completed At: {automatedTaskInstanceDetail.completedAt}";

                    if (automatedTaskInstanceDetail.actionExecutionDetails.FirstOrDefault().issues?.Count > 0)
                    {
                        message += ($"\n\n Failure Reasons: \n");
                        foreach (IssueDetail issue in automatedTaskInstanceDetail.actionExecutionDetails.FirstOrDefault().issues)
                        {
                            if (automatedTaskInstanceDetail.configurationType == ConfigurationType.Application)
                                message += ($"\n\n - {issue.resourceType}: {issue.failureReason} \r ");
                            else
                                message += ($"\n\n - {issue.failureReason} \r ");
                        }
                    }
                    if (automatedTaskInstanceDetail.actionExecutionDetails.FirstOrDefault().info?.Count > 0)
                    {
                        message += ($"\n\n Additional Information: \n");
                        foreach (IssueDetail info in automatedTaskInstanceDetail.actionExecutionDetails.FirstOrDefault().info)
                        {
                            if (automatedTaskInstanceDetail.configurationType == ConfigurationType.Application)
                                message += ($"\n\n - {info.resourceType}: {info.failureReason} \r ");
                            else
                                message += ($"\n\n - {info.failureReason} \r ");
                        }
                    }
                }
                else if (automatedTaskInstanceDetail.configurationType == ConfigurationType.CustomWorkflow)
                {
                    message += ($"\n Resource Type: {automatedTaskInstanceDetail.configurationType}" +
                      $"\n\n Overall Status: {automatedTaskInstanceDetail.taskStatus} " +
                      $"\n\n Execution Type: {automatedTaskInstanceDetail.executionType} " +
                      $"\n\n Started At: {automatedTaskInstanceDetail.startedAt} " +
                      $"\n\n Completed At: {automatedTaskInstanceDetail.completedAt}");

                    foreach (ActionExecutionDetails actionExecutionDetail in automatedTaskInstanceDetail.actionExecutionDetails)
                    {
                        List<GroupedResources> deserializedGroupedResources = JsonConvert.DeserializeObject<List<GroupedResources>>(actionExecutionDetail.groupedResourcesList);
                        message += "\n----------------------------------------------------------------------------------------------------\n";
                        message += ($"\n Step {actionExecutionDetail.configuredOrder} - {actionExecutionDetail.actionPerformed}" +
                        $"\n\n Status: {actionExecutionDetail.actionStatus} ");
                        message += ($"\n Resource Type: {actionExecutionDetail.resourceType} "
                            + $"\n\n Resources Detail: ");
                        foreach (GroupedResources group in deserializedGroupedResources)
                        {
                            message += actionExecutionDetail.resourceType != AutomatedTaskConstants.PowerShellScript ? $"\n {group.groupName} \n" : $"\n {group.groupName} - ";
                            message += $" {string.Join(", ", group.groupMembers)} ";
                            message += $"\n\n";
                        }

                        if (actionExecutionDetail.issues?.Count > 0)
                        {
                            message += ($"\n\n Failure Reasons: \n");
                            foreach (IssueDetail issue in actionExecutionDetail.issues)
                            {
                                message += ($"\n\n - {issue.failureReason} \r ");
                            }
                        }
                        if (actionExecutionDetail.info?.Count > 0)
                        {
                            message += ($"\n\n Additional Information: \n");
                            foreach (IssueDetail info in actionExecutionDetail.info)
                            {
                                message += ($"\n\n - {info.failureReason} \r ");
                            }
                        }
                    }
                }
                #endregion

                return SendMessageToNotificationChannel();
            }
            catch (Exception ex)
            {
                LoggingHelper.Info($"Custom channel notification failed for Automated Task {automatedTaskInstanceDetail.taskName}. Error " + ex.Message);
                return false;
            }

        }

        // Implement the logic to connect with channel and send the constructed message.
        public bool SendMessageToNotificationChannel()
        {
            try
            {
                // Save / Send  -- Your Operation Goes here
                //Returns the status
                return true;
            }
            catch (Exception ex)
            {
                LoggingHelper.Info($"Custom channel notification failed. Error " + ex.Message);
                return false;
            }
        }
    }
}