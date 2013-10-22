choiceview-webapi-twilio-demo
=============================

A demo showing how to use the ChoiceView API with the Twilio API to create a visually enabled IVR on the Twilio platform. The ChoiceView REST API is a REST-based service that enables visual capabilities on new and existing IVR systems. A ChoiceView-equipped IVR provides visual menus and visual responses to callers. If live assistance is needed, it can transfer the call to a contact center agent with payload delivery and continued visual sharing.
This repository contains source code for a ChoiceView implementation on the Twilio platform. On the Twilio server you need to configure the "Voice Request URL" to point to your web site and make sure that your web site is enabled to allow Post requests.

The application consists of two parts: a c# dll which interfaces with the ChoiceView API and a Webmatrix project consisting of xml and cshtml files which interface with the Twilio API.

The demo begins with the start.xml file which plays the ChoiceView signature prompt and then redirects to launchCV.cshtml. This file calls the cvWorkThread in the CVClassLib dll to notify the 
ChoiceView Server of the incoming session. It then redirects to Start2.mxl to play the initial prompt to the caller. Control will stay in that module until the caller activates the ChoiceView
app on their mobile device. If the caller does not activate the Choiceview app, there are a series of xml files which control a voice only flow for the caller. If the caller activates 
ChoiceView on their mobile device, the ChoiceView Server will respond to the incoming session notification which will cause the thread to redirect the Twilio app to the ChoiceView main Menu. 
At this point, all of the Twilio voice prompts will be controlled by notifications sent from the Choiceview Server to the stateChangeURI and newmessageURI values passed when the session was 
established. In this demo, the URIs point to new_message.cshtml and state_change.cshtml. These two files pass the necessary info to a work thread in the dll to parse the messages and control 
the flow based upon the caller's choices on their mobile device. Each of the choices on the mobile device menu results in a menu being sent to the ChoiceView Server to be sent to the device 
and a corresponding voice prompt being sent to the Twilio Server thus keeping the two servers in sync.
