choiceview-webapi-twilio-demo
=============================

A demo showing how to use the ChoiceView API with the Twilio API to create a visually enabled IVR on the Twilio platform. The ChoiceView REST API is a REST-based service that enables visual capabilities on new and existing IVR systems. A ChoiceView-equipped IVR provides visual menus and visual responses to callers. If live assistance is needed, it can transfer the call to a contact center agent with payload delivery and continued visual sharing.

Overview
--------

ChoiceView is a Communications-as-a-Service (CAAS) platform consisting of a protocol, switching mechanism and corresponding software applications to allow smartphone users to receive and send visual information and data during an ordinary phone call.  In essence, ChoiceView joins a voice call with an associated data connection to allow a caller to speak with another party while that party shares relevant visual information in real time.
 
The goal of ChoiceView is to easily add enhanced voice/data capabilities to the huge installed based of telephony switches, IVRs, mobile devices and other network endpoints.


Description
-----------

This repository contains source code for a ChoiceView implementation on the Twilio platform. On the Twilio server you need to configure the "Voice Request URL" to point to your web site and make sure that your web site is enabled to allow Post requests.

The application consists of two parts: a c# dll which interfaces with the ChoiceView API and a Webmatrix project consisting of xml and cshtml files which interface with the Twilio API.

The demo begins with the start.xml file which plays the ChoiceView signature prompt and then redirects to launchCV.cshtml. This file calls the cvWorkThread in the CVClassLib dll to notify the 
ChoiceView Server of the incoming session. It then redirects to Start2.xml to play the initial prompt to the caller. Control will stay in that module until the caller activates the ChoiceView
app on their mobile device. If the caller does not activate the Choiceview app, there are a series of xml files which control a voice only flow for the caller. If the caller activates 
ChoiceView on their mobile device, the ChoiceView Server will respond to the incoming session notification which will cause the thread to redirect the Twilio app to the ChoiceView main Menu. 
At this point, all of the Twilio voice prompts will be controlled by notifications sent from the Choiceview Server to the stateChangeURI and newmessageURI values passed when the session was 
established. In this demo, the URIs point to new_message.cshtml and state_change.cshtml. These two files pass the necessary info to a work thread in the dll to parse the messages and control 
the flow based upon the caller's choices on their mobile device. Each of the choices on the mobile device menu results in a menu being sent to the ChoiceView Server to be sent to the device 
and a corresponding voice prompt being sent to the Twilio Server thus keeping the two servers in sync.

LICENSE
-------
[MIT License](https://github.com/radishsystems/choiceview-webapi-java/blob/master/LICENSE)


Building the Application
------------------------

The Webmatrix portion does not need to be built. The c# dll should build "as-is" with Microsoft Visual Studio. However,
you will need a account and password in order to actually use the demo.


Running the Application
-----------------------

To use the TwilioRadishDemo application, you must have a mobile device with the latest ChoiceView client installed. 
You should know the phone number of the mobile device, or the phone number that the ChoiceView client is configured 
to use. The client must be configured to use the ChoiceView development server. On iOS devices, press Settings, 
then Advanced, then change the server field to cvnet2.radishsystems.com. On Android devices, press the menu button, 
then Settings, then scroll down to the server field and change it to cvnet2.radishsystems.com.

Then call the phone number associated with the Twilio platform. You will be prompted to press the "Start" button 
on your mobile device client. If configured correctly, you should see "ChoiceView Agent Connected" and you will be 
presented with the Main Menu. Now, you can tap a selection on the mobile client as well as interact with the Voice 
Application.

Contact Information
-------------------
If you want more information on ChoiceView, or want access to the ChoiceView REST API, contact us.

[Radish Systems, LLC](http://www.radishsystems.com/support/contact-radish-customer-support/)

-	support@radishsystems.com
-	darryl@radishsystems.com
-	+1.720.440.7560
