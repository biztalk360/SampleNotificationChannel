// Notes: 

1. Solution Items Folder has three Dll's, which need to be referenced into the NC project.
	    I. B360.Notifier.Common.dll
	   II. B360.Common.EntityObjects.dll
	  III. Newtonsoft.Json.dll

2. GlobalProperties.json - This file outlines the input fields required for configuring Global Settings for the Channel.
   AlarmProperties.json - This file outlines the input fields required for configuring Alarm Settings for the Channel.

3. "AlarmProperties.json" and "GlobalProperties.json" files must be a embedded resource files.
		 I. Right click on the File and click on the Properties.
		II. Make sure the "Build Action" is set to "Embedded Resource". 

3. Your Channel class must inherit the IChannelNotification Interface.
		I . Like : public class SampleNotificationChannel : IChannelNotification<T>

4. "SendNotification()" - Method to Send Alarm Notification 

5. "SendAnalyticsReport()" - Method to Send Analytics Dashboard and Secure SQL Query as a Report.

6. "SendAutomatedTaskSummary()" - Method to Send Automated Task Execution Result.

7. "SendMessageToNotificationChannel()" - Implement the logic to connect with channel and send the constructed message.