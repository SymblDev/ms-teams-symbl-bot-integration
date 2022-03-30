This is a POC which shows how to integrate [Symbl.ai](https://symbl.ai/) with MS Teams and getting per participant the meeting conversation transcripts and insights in real-time using an existing sample application created by MS Platform for Situated Intelligence ([Psi](https://github.com/microsoft/psi)) Teams bot. You can find more details on the original code created by MS Psi Teams bot sample application here microsoft-graph-comms-samples.

The MS Teams Symbl Integration Guide focuses on the local development where you are free to use your "Windows" machine for development/testting or may use the Azure infrastructure too. Up next, you'll see a dedicated section for "Azure" Development; Where you can spin up the Azure VM and develop or deploy the solution.

## Local Development Guide

<details>
<summary>Click to expand!</summary>
  

In this playbook, you will be presented with the MS Teams and Sybml integration to get real-time transcriptions and insights using Symbl APIs. We will be leveraging and extending the Psi Bot which is an open-source MS Teams Bot implementation developed by MS [Platform for Situated Intelligence (\psi)](https://github.com/microsoft/psi).

Before taking a deep dive into the internals of the MS Teams Bot implementation, let&#39;s try to understand what is a &quot;Bot&quot;, then get a basic understanding of the Psi Bot, Symbl API capabilities etc.

## What is a bot?

&quot;_Bots provide an experience that feels less like using a computer and more like dealing with a person - or at least an intelligent robot. They can be used to shift simple, repetitive tasks—such as taking a dinner reservation or gathering profile information—onto automated systems that may no longer require direct human intervention. Users converse with a bot using text, interactive cards, and speech. A bot interaction can be a quick question and answer, or it can be a sophisticated conversation that intelligently provides access to services._

_A bot can be thought of as a web application that has a conversational interface. A user connects to a bot through a channel such as Facebook, Slack, or Microsoft Teams._

- _The bot reasons about input and performs relevant tasks. This can include asking the user for additional information or accessing services on behalf of the user._
- _The bot performs recognition on the user&#39;s input to interpret what the user is asking for or saying._
- _The bot generates responses to send to the user to communicate what the bot is doing or has done._
- _Depending on how the bot is configured and how it&#39;s registered with the channel, users can interact with the bot through text or speech, and the conversation might include images and video.&quot;_

Reference -

[https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview?view=azure-bot-service-4.0](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview?view=azure-bot-service-4.0)

### Introduction - Symbl.ai

Symbl&#39;s APIs empower developers to enable:

[Real-time](https://docs.symbl.ai/docs/streaming-api/api-reference#messages) analysis of free-flowing discussions to automatically surface highly relevant summary discussion topics, contextual insights, suggestive action items, follow-ups, decisions, and questions.

[Streaming API](https://docs.symbl.ai/docs/streamingapi/introduction) that makes it easy to add AI-powered conversation intelligence using WebSocket interface.

[Conversation APIs](https://docs.symbl.ai/docs/conversation-api/introduction) that provide a REST interface for managing and processing your conversation data.

[Summary UI](https://docs.symbl.ai/docs/api-reference/experience-api/post-video-summary-ui) with a fully customizable and editable reference experience that indexes a searchable transcript and shows generated actionable insights, topics, timecodes, and speaker information.

### Introduction - Psi Teams Bot

Teams bots can be developed which participate in Teams meetings much the same way humans do; joining calls, consuming participant video and audio streams, and producing their own audio and video as well as screen-sharing streams.

[Platform for Situated Intelligence (\psi)](https://github.com/microsoft/psi) is an open, extensible framework for the development and research of multimodal, integrative-AI systems. The framework is particularly well-suited for developing AI systems that perform audio-visual processing in real-time interactions.

In order to help accelerate the development of Teams bots with real-time audio and video capabilities, we have created this sample application which shows how you can integrate \psi with the Teams bot architecture to develop bots that can participate in live meetings, [visualize and debug](https://github.com/microsoft/psi/wiki/Psi-Studio) your implementation, and iterate quickly offline from persisted data.

This is a sample MS Teams bot implementation and may be used as a starting point for creating bots of your own with \psi. In particular, this sample consumes participant audio and video streams and visualizes engagement by producing a screen-share video stream with a ball gravitating towards the most recently active speakers (partially inspired by [this research](https://www.media.mit.edu/publications/meeting-mediator-enhancing-group-collaboration-and-leadership-with-sociometric-feedback/)). This sample was announced at a virtual Platform for Situated Intelligence workshop, and the recording of that session can be found [here](https://youtu.be/7Wh4Xr1Bazg?t=8582).

###


## PSI Bot Architectural Overview

The following projects comprise this sample:

- [**PsiBot**](https://github.com/SymblDev/ms-teams-symbl-bot-integration/tree/main/PsiBot.Service) - This is the bot itself. It contains boilerplate code that handles all the mechanics of hosting a Teams bot. This is also where we embed our \psi implementation (inside the [PsiBot.Service](https://github.com/SymblDev/ms-teams-symbl-bot-integration/tree/main/PsiBot.Service) project).
- [**TeamsBot**](https://github.com/SymblDev/ms-teams-symbl-bot-integration/tree/main/TeamsBot) - This project simply contains the ITeamsBot interface, which is used to create your \psi pipeline that can be plugged into the Teams bot infrastructure directly (i.e., as a drop-in replacement inside [PsiBot.Service](https://github.com/SymblDev/ms-teams-symbl-bot-integration/tree/main/PsiBot.Service) or used in offline testing ([TeamsBotTester](https://github.com/SymblDev/ms-teams-symbl-bot-integration/tree/main/TeamsBotTester)).
- [**TeamsBotSample**](https://github.com/SymblDev/ms-teams-symbl-bot-integration/tree/main/TeamsBotSample) - Contains two sample implementations of the ITeamsBot interface. Both of these examples consume participant audio and video and visualize engagement by producing a screen-share video stream, either via a &quot;bouncing ball&quot; or by scaling video thumbnails.

##


## High-Level Architecture of MS Teams Bot Symbl Integration

![HighLevelArchitecture-MSTeamsSybml](https://user-images.githubusercontent.com/2565797/160795930-c680692b-aa82-484e-b90b-1d1003f5d88f.png)

## Prerequisites

- [Office 365 tenant](https://developer.microsoft.com/microsoft-365/dev-program)
- [Ngrok](https://ngrok.com/) or equivalent tunneling solution
- [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/)or later
- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1
- Microsoft Teams with an account (not a guest account)
- Azure Account
- Git 64 Bit (Optional to Clone the Github Repo)

#### **Symbl**

If you haven&#39;t signed up for an account with Symbl, please follow [https://symbl.ai](https://symbl.ai/) to get started.

Log on to the Symbl Platform website ([http://platform.symbl.ai](http://platform.symbl.ai/)) and get the App Id and App Secret Key.

 **Azure**

For the MS Teams Bot Symbl integration, it&#39;s mandatory to have the Azure Account. If you haven&#39;t signed up, please follow this link [https://portal.azure.com/](https://portal.azure.com/)

Please make sure to have a valid subscription on Azure. The Azure Trial account won&#39;t work. Please go with the Pay as you go subscription.

#### **Windows**

Visual Studio 2019 or later is required to develop, build and publish the PsiBot.sln solution on Windows.

**Setup Visual Studio 2019 or later** :

Install [Visual Studio 2019](https://www.visualstudio.com/vs/). The Community Edition of Visual Studio is sufficient. Make sure the following features are installed (you can check these features by running the Visual Studio Installer again and looking at both the Workloads and Individual components tabs):

Workloads:
  - .NET desktop development
  - .NET Core cross-platform development

##


## Getting started

If you are not familiar with Platform for Situated Intelligence, you&#39;ll first want to acquaint yourself with some fundamental concepts. The [GitHub page](https://github.com/microsoft/psi) has lots of documentation for familiarizing yourself with the framework, including a [wiki](https://github.com/microsoft/psi/wiki), [samples](https://github.com/Microsoft/psi-samples), [tutorials](https://github.com/microsoft/psi/wiki/Tutorials), and more. A great place to get started is the [Brief Introduction](https://github.com/microsoft/psi/wiki/Brief-Introduction) tutorial.

In the next steps, we&#39;ll walk through how to initialize your bot and other necessary resources in Azure. Following these instructions will enable you to execute the bot on your local development machine.

## Step 1: Setting up the Source Code

This step is for setting up the source code and compiling the same on your local machine.

Note - In-order to compile the source code, you need to make sure to install Visual Studio as explained previously under **Setup Visual Studio 2019 or later.**

Please make sure to clone or download the following repo on your local machine. Make sure to open the project, compile or build the same to confirm there are no errors.

[https://github.com/symblai/ms-teams-symbl-bot-integration](https://github.com/symblai/ms-teams-symbl-bot-integration)

##


## Step 2 : Azure App Registration

Registering your application establishes a trusting relationship between your app and the Microsoft identity platform. The trust is unidirectional: your app trusts the Microsoft identity platform, and not the other way around.

 Follow these steps to create the app registration:

1. Sign in to the [Azure portal](https://portal.azure.com/).
2. If you have access to multiple tenants, use the **Directories + subscriptions** filter ![](RackMultipart20220330-4-vlzoo_html_f53e57e5486137cc.png) in the top menu to switch to the tenant in which you want to register the application.
3. Search for and select **Azure Active Directory**.
4. Under **Manage** , select **App registrations** \&gt; **New registration**.
5. Enter a display **Name** for your application. Users of your application might see the display name when they use the app, for example during sign-in. You can change the display name at any time and multiple app registrations can share the same name. The app registration&#39;s automatically generated Application (client) ID, not its display name, uniquely identifies your app within the identity platform.
6. Specify who can use the application, sometimes called its _sign-in audience_.

![AzureSupportedAccountTypes](https://user-images.githubusercontent.com/2565797/160796309-d7e96010-a8d5-47d7-bb36-c2a22542bec2.png)

1. Don&#39;t enter anything for **Redirect URI (optional)**. You&#39;ll configure a redirect URI in the next section.
2. Select **Register** to complete the initial app registration.

When registration finishes, the Azure portal displays the app registration&#39;s **Overview** pane. You&#39;ll see the **Application (client) ID**, Also called the _client ID_, this value uniquely identifies your application in the Microsoft identity platform. Please make a note of this Client ID.

## **Add Credentials**

Credentials are used by [confidential client applications](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-client-applications) that access a web API. Examples of confidential clients are web apps, other web APIs, or service-type and daemon-type applications. Credentials allow your application to authenticate itself, requiring no interaction from a user at runtime.

![AddCredentials](https://user-images.githubusercontent.com/2565797/160797145-ab95fce0-5865-4466-b0e9-ec48fb3a8f1b.png)

1. In the Azure portal, in **App registrations** , select your application.
2. Select **Certificates &amp; secrets** \&gt; **Client secrets** \&gt; **New client secret**.
3. Add a description of your client secret.
4. Select an expiration for the secret or specify a custom lifetime.
  - Client secret lifetime is limited to two years (24 months) or less. You can&#39;t specify a custom lifetime longer than 24 months.
  - Microsoft recommends that you set an expiration value of fewer than 12 months.
5. Select **Add**.
6. _Record the secret&#39;s value_ for use in your client application code. This secret value is _never displayed again_ after you leave this page.

## **Manage API Permissions**

Make sure that your account can grant admin consent for Microsoft. After adding permissions, select Grant admin consent for Microsoft to know the status of the consent.

![ManageAPIPermissions](https://user-images.githubusercontent.com/2565797/160798084-3413cf2f-e40a-4655-8707-2d69206c8bb3.png)

1. Select **API permissions**.
2. Select **Add permission**. **Request API permissions** window appears.
3. Select **Microsoft APIs** and select **Microsoft Graph**.
4. Select **Application permissions** and then select permissions.
5. Add the following Graph API Applications permissions to your Azure App and grant admin consent.
  - Calls.AccessMedia.All
  - Calls.Initiate.All
  - Calls.JoinGroupCall.All
  - Calls.JoinGroupCallAsGuest.All
6. Select **Add permissions**
7. Grant admin consent for each of the above-mentioned API Permissions.

## Step 3 - Create bot channel registration in Azure account

Single-tenant apps are only available in the tenant they were registered in, also known as their home tenant.

Multi-tenant apps are available to users in both their home tenant and other tenants.

**Best practices for multi-tenant apps**

Building great multi-tenant apps can be challenging because of the number of different policies that IT administrators can set in their tenants. If you choose to build a multi-tenant app, follow these best practices:

- Test your app in a tenant that has configured [Conditional Access policies](https://docs.microsoft.com/en-us/azure/active-directory/azuread-dev/conditional-access-dev-guide).
- Follow the principle of least user access to ensure that your app only requests permissions it actually needs.
- Provide appropriate names and descriptions for any permissions you expose as part of your app. This helps users and admins know what they&#39;re agreeing to when they attempt to use your app&#39;s APIs. For more information, see the best practices section in the [permissions guide](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-permissions-and-consent).

[https://docs.microsoft.com/en-us/azure/active-directory/develop/single-and-multi-tenant-apps](https://docs.microsoft.com/en-us/azure/active-directory/develop/single-and-multi-tenant-apps)

1. Open [Azure portal](https://portal.azure.com/).
2. Select **Create a resource**.
3. Search for **Azure Bot** in the search box.
4. Select **Azure Bot**.
 ![AzureBot](https://user-images.githubusercontent.com/2565797/160798689-aa540086-fc9f-48d2-9e39-ce2f580f417d.png)
5. Select **Create**.
6. Enter bot handle name in **Bot handle** field.
7. Select your **Subscription** from the dropdown list.
8. Select your **Resource group** from the dropdown list.
 To create a new resource group, select **Create new** , enter resource name, select **OK** , and select a required location from **New resource group location** dropdown list.
 ![CreateAnAzureBot](https://user-images.githubusercontent.com/2565797/160798884-c2df69e8-1dd9-4e3e-969a-f0ed89e3571a.png)
9. Select **Type of App** for ex: **Multi Tenant** for **Microsoft App ID**. Please make sure to select the appropriate type based on what you have chosen during the App Registration section in Step 1.

10. Select **Use existing app registration** and enter **App ID** and **App secret** of your azure app registered in your tenant.
11. For the pricing, you can choose the **Standard F0 Free** one.
12. Select **Review + create**.
![ReviewAndCreateAzureBot](https://user-images.githubusercontent.com/2565797/160799213-03865dcf-9517-4243-9c1a-2b4371ee0380.png)

**Note - The App ID and the App Secret shown in the screenshot is for reference purpose only. Please make sure to use the valid ones that you have registered.**

1. If the validation passes, select **Create**. It takes a few moments for your bot service to be provisioned.
 ![CreateAzureBotValidationPassed](https://user-images.githubusercontent.com/2565797/160799404-c4b89aab-b5ba-475f-b62d-852ade5029f4.png)
2. Select **Go to resource**. The bot and the related resources are listed in the resource group.
Now your Azure bot has been created.
 ![AzureBotGotoResources](https://user-images.githubusercontent.com/2565797/160799543-a896ed98-f0fc-4ae0-8ed4-72afbb69a8f3.png)

## Step 4 - Setting up a custom domain

We need to have a dedicated domain registered for this integration. You have the following choice. Either go with the existing domain registered via the domain provider or go with the Azure App Service Domain.

If you don&#39;t have the domain, you can purchase a domain name from Azure. This is like creating any other resource in the Azure portal. Create a resource called &quot; **App Service Domain**&quot; and give it a name (e.g., &quot;mydomain.com&quot;).

![AppServicesDomain](https://user-images.githubusercontent.com/2565797/160800667-e5334876-2223-4fc6-afa4-56d379053782.png)

Please make sure to select the existing or newly created &quot;App Service Domain&quot; to add a Record Set with the name as local, Type as CNAME, and the value as 0.tcp.ngrok.io as shown below.

![DNSZone](https://user-images.githubusercontent.com/2565797/160800892-1a041cd2-a740-4022-86d1-f810fb499124.png)

##


## Step 5 - Generate an SSL certificate for your domain

We are going to make use of certbot and openssl for generating the certificate for the required domain. One can obtain a free certificate via [https://letsencrypt.org](https://letsencrypt.org/). Please follow the below links to install or setup the certbot and openssl on your Windows Development machine.

[https://sourceforge.net/projects/certbot.mirror/files/latest/download](https://sourceforge.net/projects/certbot.mirror/files/latest/download)

[https://slproweb.com/products/Win32OpenSSL.html](https://slproweb.com/products/Win32OpenSSL.html)

Execute the below command to generate the wildcard certificate.

**certbot certonly --manual --preferred-challenges dns -d \*.<domain_name>**

This is to create the wildcard certificate for your domain, e.g., \*.mydomain.com.

The above command will ask to create a DNS TXT record under the name. Copy the TXT value (do not press enter).

In your App Service Domain, [Create a TXT record](https://docs.microsoft.com/en-us/azure/app-service/app-service-web-tutorial-custom-domain#create-the-cname-record) by specifying:

- Name: \_acme-challenge

- Type: TXT

- TTL: 3600

- Value: \&lt;as copied TXT value from Terminal\&gt;

![DNSZone](https://user-images.githubusercontent.com/2565797/160800892-1a041cd2-a740-4022-86d1-f810fb499124.png)

Please make use of the following command for generating the private certificate key file for Windows assuming you have successfully completed the certbot for generating the wildcard certificate.

**openssl pkcs12 -export -out <certificate_name>.pfx -inkey <path_to_cert_files>/privkey.pem -in <path_to_cert_files>/cert.pem**

Alternatively, If you have the PEM file, you can generate the cer file by making use of the PEM file.

**openssl x509 -inform PEM -in pkcs12.pem -outform DER -out certificate.cer**

The following command is used for exporting the private certificate key by making use of the PEM and CER files.

**openssl pkcs12 -export -out certificate.pfx -inkey privateKey.pem -in certificate.cer**

![OpenSSLCert](https://user-images.githubusercontent.com/2565797/160802887-5ed9f3bf-6edc-437a-b1fc-e1e7ca0e2227.png)

Breaking down the command:

openssl – the command for executing OpenSSL

pkcs12 – the file utility for PKCS#12 files in OpenSSL

-export -out certificate.pfx – export and save the PFX file as certificate.pfx

-inkey privateKey.key – use the private key file privateKey.key as the private key to combine with the certificate.

-in certificate.crt – use certificate.crt as the certificate the private key will be combined with.

In your App Services Domain, map the CNAME entry so any request to **local.{mydomain}.com** will be redirected to 0.tcp.ngrok.io.

Install the generated certificate into your system certificate manager. Look it up in the manager and take note of the THUMBPRINT for later.Step

## Step 6 - Setting up the private certificate

Once the PFX certificate is generated from Step 5, the next step is to install the same on your local &quot;Windows&quot; development machine.

1. Double click on the private certificate that you have generated.
2. Select Local Machine as shown below and click &quot;Next&quot;.

![](RackMultipart20220330-4-vlzoo_html_27c6e984749f1827.png)

3. Again click on &quot;Next&quot;

![CertificateImport1](https://user-images.githubusercontent.com/2565797/160803404-0fd6d724-8034-4fbc-8b8a-43468b601fd9.png)

4. Make sure to provide the password that you have specified during the certificate creation process in Step 5.

![CertificateImport2](https://user-images.githubusercontent.com/2565797/160803566-0577cd29-04b7-4830-bc67-1c5df22f138f.png)

5. Select location as &quot;Personal&quot; and click on &quot;Next&quot; to proceed.

![CertificateImport3](https://user-images.githubusercontent.com/2565797/160803657-969c09bf-f032-4acb-9d9a-786ff6f2b0aa.png)

![CertificateImport4](https://user-images.githubusercontent.com/2565797/160803779-eb478684-bd04-4e55-884b-7637e5f4751f.png)

![CertificateImport5](https://user-images.githubusercontent.com/2565797/160806478-2a15b8a5-926e-4e4f-a34a-696f35d2d5cf.png)

6. Do Control -> R and then type the following command - certlm.msc and hit enter to launch the certificate management UI.
7. Go to the "Personal" folder to find the certificate that you have installed.
8. Select and Double click on the certificate to view the details and make a note on the certificate thumbprint and subject name.

## Step 7 - Setting up ngrok

1. Login or Signup for a free [ngrok](https://ngrok.com/) account and get your auth token.
2. Create an ngrok config file as follows. Replace with the ngrok authentication token. Save it as, e.g., &quot;ngrok.yml&quot;.

```
authtoken: YOUR_TOKEN
tunnels:
signaling:
   addr: https://localhost:9441
   proto: http
media:
   addr: 8445
   proto: tcp
```

1. Open a command prompt (you may need to run as an Administrator) and start ngrok: **ngrok start --all --config ngrok.yml**
![ngrokRun](https://user-images.githubusercontent.com/2565797/160804221-b1e19fca-5996-4037-9986-264cdbd14860.png)

1. Note down the following things from ngrok output
  - HTTP forwarding url (in this example: **c6bd-40-76-82-252.ngrok.io** )
  - The port (in this example: **18356** ).
2. If you followed the above steps and only have one subdomain mapped to 0.tcp.ngrok.io, and

Note - if ngrok did not assign a **0.tcp address** , then just re-run it until it does.

## Step 8 - AppSettings.json changes on PsiBot.Service

To execute the PsiBot sample (Based on Step 1 the source code that was downloaded), the appsettings.json file in the **PsiBot.Service** project, has to be manually updated with the required configurations as shown below.

```
"BotConfiguration": {
    "BotName": "<bot_name>",
    "AadAppId": "<app_id>",
    "AadAppSecret": "<app_secret>",
    "ServiceCname": "{ngrok-instance}.ngrok.io",
    "MediaServiceFQDN": "local.{mydomain}.com",
    "ServiceDnsName": "",
    "CertificateSubjectName": "CERTIFICATE_SUBJECTNAME",
    "CertificateThumbprint": "CERTIFICATE_THUMBPRINT",
    "InstancePublicPort": <ngrok port>,
    "CallSignalingPort": 9441,
    "InstanceInternalPort": 8445,
    "PlaceCallEndpointUrl": "https://graph.microsoft.com/v1.0",
    "PsiStoreDirectory": "<optional directory for persisted store>",
    "SybmlAppId": "<Symbl AppId>",
    "SybmlAppSecret": "<Symbl appSecret>"
  }
```

- Fill in the **BotName** which is the Azure Bot Resource Name.
- Fill in the **AadAppId** which is nothing but the Registered Application Client Id which can be obtained from Step 2.
- Fill in the **AadAppSecret** which is nothing but the Registered Application Client Secret Key, which can be obtained from Step 2.
- Fill in the **CertificateThumbprint** with the value that you have noted earlier during the certificate install, which can be obtained from Step 6.
- Fill in the **CertificateSubjectName** with the value that is obtained from the installed certificate, which can be obtained from Step 6.
- Fill in the **ServiceCname** with ngrok&#39;s http address, which you can obtain from Step 7.
- Fill in **InstancePublicPort** with the ngrok tcp assigned port, which can be obtained from Step 7.
- **MediaServiceFQDN** is the subdomain URL that is forwarded to the tcp URL assigned by ngrok (e.g., local.{mydomain}.com is forwarded to 0.tcp.ngrok.io). Note - mydomain.com is the registered custom domain as per Step 4.
- Fill in the **SybmlAppId** and **SymblAppSecret** with the value that can be obtained from the Symbl Portal - [https://platform.symbl.ai/](https://platform.symbl.ai/)

##


## Step 9 - Running the Bot

You should now have everything you need to run the bot and have it join a meeting.

1. Run Visual Studio As Administrator.
2. Do File - Open -> Project/Solution.
3. Navigate to the folder where you have cloned or downloaded and select the **ms-teams-symbol-bot-integration** project. 

![RunningTheBot](https://user-images.githubusercontent.com/2565797/160805890-91eed72f-2387-45d3-9f1b-b204ac61e093.png)

4. Run the [PsiBot.Service](https://github.com/SymblDev/ms-teams-symbl-bot-integration/blob/main/PsiBot/PsiBot.Service/PsiBot.Service.csproj) project (in Kestrel, not IIS Express). Please make sure the following

1. The PsiBot.Service project is &quot;Set as a Startup Project&quot;. If not. Please make sure to do so by right clicking on the PsiBot.Service project.
2. PsiBotService is set while debugging or running the project.

![ProjectDebug](https://user-images.githubusercontent.com/2565797/160804699-c381d2ed-5e59-427d-9fbb-678187f53dd8.png)

1. Create an MS Teams Meeting and copy the meeting link.
2. Navigate to the ngrok-instance URL. You can obtain the instance from **Step 6**. Go to **{ngrok-instance}**.ngrok.io/manage in a browser, to have your bot join and leave specific meeting instances.
3. Paste the meeting link and click on the &quot;Join Meeting&quot; button. It will take a few seconds for the Bot to join the Teams Meeting. If all goes well, you should be able to see the &quot;Bot&quot; joining the meeting.
4. As and when the speaker(s) is communicating the live transcription should be displayed on the console. Simultaneously, it&#39;s possible to also subscribe from the browser and get the necessary information for making the **Symbl Subscribe API** (https://docs.symbl.ai/docs/subscribe-api).

![TeamsBotUI](https://user-images.githubusercontent.com/2565797/160804825-b4377c4d-5910-457b-a95d-63c966ad49a3.png)

## Azure Bot Pricing

Please check the below Azure Bot Pricing Guide to know more about the Azure Bot Service Pricing.

[https://azure.microsoft.com/en-gb/pricing/details/bot-service/](https://azure.microsoft.com/en-gb/pricing/details/bot-service/)

**What are standard channels?**

Standard channels include Microsoft first-party services (such as Skype, Cortana and Microsoft Teams) and services with publicly available Bot APIs (such as Facebook and Slack). Please refer to the Bot Service documentation for the complete list.

**What are premium channels?**

The premium channels allow your bot to reliably communicate with users within your own application or on your website. These channels allow you to customize the client experience for your users by customizing the open source DirectLine and Web Chat clients. Please refer to the Bot Service documentation for details.
  
</details>

## Azure Development Guide

<details>
  <summary>Click to expand!</summary>
    

In this playbook, you will be presented with the MS Teams and Sybml integration to get real-time transcriptions and insights using Symbl APIs. We will be leveraging and extending the Psi Bot which is an open-source MS Teams Bot implementation developed by MS [Platform for Situated Intelligence (\psi)](https://github.com/microsoft/psi).

Please make sure to follow the **MS Teams - Symbl Integration - Local Development Guide** to perform the mandatory setups on Azure.

**Note** -

- The only difference between Local and Azure development is that the source code is set-up on the Azure VM, plus the DNS resource changes for accessing the MS Teams bot application via the custom configured domain.
- For development or debugging, the extended PsiBot source code setup on Azure is necessary. However, only the executables need to be deployed for production.
- TCP Tunneling - Signaling and Media Stream relies heavily on Ngrok. Regardless of whether you choose a Local or Azure-based implementation, you&#39;ll need to use &quot;Ngrok&quot; or another tunneling solution.
- This playbook shows how to implement a single VM-based solution. However, a load-balanced solution must be in place when you go live, taking into account the load and stability factors.

##
# **Step 1: Create a Windows virtual machine in the Azure portal**

&quot;_Azure virtual machines (VMs) can be created through the Azure portal. This method provides a browser-based user interface to create VMs and their associated resources. This quickstart shows you how to use the Azure portal to deploy a virtual machine (VM) in Azure that runs Windows Server 2019. To see your VM in action, you then RDP to the VM and install the IIS web server.&quot;_

1. Type **virtual machines** in the search.
2. Under **Services** , select **Virtual machines**.
3. In the **Virtual machines** page, select **Create** and then **Virtual machine**. The **Create a virtual machine** page opens.
4. In the **Basics** tab, under **Project details** , make sure the correct subscription is selected and then choose to **Create a new** resource group. Type myResourceGroup for the name.
5. Under **Instance details** , type _the VM name_ for the **Virtual machine name** and choose _Windows 10 Pro_ for the **Image**. You may select Windows Server too.

![CreateVM1](https://user-images.githubusercontent.com/2565797/160810364-27139832-16ea-42af-aaa5-261c926ee158.png)

6. Under the **Administrator account** , provide a username, such as _azureuser_ and a password. The password must be at least 12 characters long and meet the [defined complexity requirements](https://docs.microsoft.com/en-us/azure/virtual-machines/windows/faq#what-are-the-password-requirements-when-creating-a-vm-).

![CreateVM2](https://user-images.githubusercontent.com/2565797/160810378-9bf6cde1-0de1-492f-8d8e-e005f0a0f963.png)

7) Under **Inbound port rules** , choose **Allow selected ports** and then select **RDP (3389)**, **HTTP (80) and HTTPS(443)** from the drop-down.

![CreateVM3](https://user-images.githubusercontent.com/2565797/160810386-37f95933-7781-4e41-affd-18fb44b01b99.png)

8) Leave the remaining defaults and then select the Review + create button at the bottom of the page.

![CreateVM4](https://user-images.githubusercontent.com/2565797/160810397-6f0673ac-f5cd-4326-b899-2edde9081774.png)

9) After validation runs, select the Create button at the bottom of the page.

10) After deployment is complete, select Go to resource.

![VMGotoResources](https://user-images.githubusercontent.com/2565797/160810723-82b67cf0-d9aa-4d3b-8ec2-394cc8e338fe.png)

You should be able to see the following Azure Resources created as part of Step 1. Note - **msteams** is an example VM name that is used for demonstration purposes.

![CreateVM5](https://user-images.githubusercontent.com/2565797/160810403-570feb81-c4d0-4b5c-b994-d7981dfa4c01.png)

## Step 2: Connect to the virtual machine

Create a remote desktop connection to the virtual machine. These directions tell you how to connect to your VM from a Windows computer. On a Mac, you need an RDP client such as this [Remote Desktop Client](https://apps.apple.com/app/microsoft-remote-desktop/id1295203466?mt=12) from the Mac App Store.

1. On the overview page for your virtual machine, select the **Connect** \&gt; **RDP**.
![CreateVM6](https://user-images.githubusercontent.com/2565797/160810408-2ce6bac9-c7f2-403d-8ea4-d615d825e425.png)
2. In the **Connect with RDP** page, keep the default options to connect by IP address, over port 3389, and click **Download RDP file**.
3. Open the downloaded RDP file and click **Connect** when prompted.
4. In the **Windows Security** window, select **More choices** and then **Use a different account**. Type the username and password you created for the virtual machine, and then click **OK**.
5. You may receive a certificate warning during the sign-in process. Click **Yes** or **Continue** to create the connection.

##

## Step 3: VM Networking Changes

Add an inbound port rule for Signaling Port 9441 and Media Stream Port 8445 as shown below.

##

![CreateVM7](https://user-images.githubusercontent.com/2565797/160810410-c48824ea-7ab0-4af4-87d7-7e40f27a67eb.png)

## Step 4: VM Public IP Address Configuration Changes

- Select the Public IP Address-based Azure Resource.
- Make sure to specify the DNS name label.
- The IP address assignment should be a Static one.
- Save the configuration.

![CreateVM8](https://user-images.githubusercontent.com/2565797/160810419-f47d188f-d110-4f66-a941-cd5ed883a04d.png)

## Step 5: DNS Zone Resource Changes

- Please make sure to select the DNS Zone based Azure Resource.
- Click on the plus icon with the Record Set to add a new record set.
- Specify the Name as &#39;@&#39;
- Specify the type as A - Alias record set
- Alias record set - Yes
- Specify the Alias type as - Azure resource
- Choose the subscription
- Specify the Azure resource with the public ip address of the Azure VM that was created in Step 1.

![CreateVM9](https://user-images.githubusercontent.com/2565797/160810426-5531000b-bf3d-4239-aeb8-8c00c9b2c0aa.png)

Click on the plus icon with the Record Set to add a new record set.

- Specify name as vm
- Type as &#39;CNAME&#39;
- Alias record set as &#39;No&#39;
- TTL as 10 seconds
- Alias as DNS Label name as per the Step 4.

![CreateVM10](https://user-images.githubusercontent.com/2565797/160810432-963d20cb-41ca-4b0b-a1d9-4dc9091b0ad9.png)

## Step 6: Setting up the Source Code

This step is for setting up the source code and compiling the same on Azure VM.

Note - In order to compile the source code, you need to make sure to install Visual Studio as explained previously under **Setup Visual Studio 2019 or later.**

Please make sure to clone or download the following repo on your local machine. Make sure to open the project, compile or build the same to confirm there are no errors.

[https://github.com/symblai/ms-teams-symbl-bot-integration](https://github.com/symblai/ms-teams-symbl-bot-integration)

## Step 7: Running the Bot

Make sure to follow the &quot; **MS Teams - Symbl Integration - Local Development Guide**&quot; Step 9 on how to run the bot, with the URL as mentioned during the run of **ngrok start --all --config ngrok.yml**

![NgrOkVM](https://user-images.githubusercontent.com/2565797/160811055-2bbff9fb-1221-4a62-918b-e5323e10fe81.png)

If you have completed Step 5, then you should be able to run the PsiBot Application with the application URL as **vm.{mydomain}.com**** /manage**

![VMTeamsSybmlAppUI](https://user-images.githubusercontent.com/2565797/160811085-c9c3f737-a174-4031-96c9-d0e1986ca656.png)
 
</details>
  
## References

[https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview?view=azure-bot-service-4.0](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview?view=azure-bot-service-4.0)

[https://docs.microsoft.com/en-us/microsoftteams/platform/sbs-calling-and-meeting](https://docs.microsoft.com/en-us/microsoftteams/platform/sbs-calling-and-meeting)

[https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app)

[https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-4.0&amp;tabs=userassigned](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration?view=azure-bot-service-4.0&amp;tabs=userassigned)

[https://microsoftgraph.github.io/microsoft-graph-comms-samples/docs/articles/Testing.html](https://microsoftgraph.github.io/microsoft-graph-comms-samples/docs/articles/Testing.html)

[https://www.ssl.com/how-to/create-a-pfx-p12-certificate-file-using-openssl/](https://www.ssl.com/how-to/create-a-pfx-p12-certificate-file-using-openssl/)

[https://knowledge.digicert.com/solution/SO26449.html](https://knowledge.digicert.com/solution/SO26449.html)

[https://docs.microsoft.com/en-us/azure/active-directory/develop/single-and-multi-tenant-apps](https://docs.microsoft.com/en-us/azure/active-directory/develop/single-and-multi-tenant-apps)
  
[https://docs.microsoft.com/en-us/azure/virtual-machines/](https://docs.microsoft.com/en-us/azure/virtual-machines/)
[https://docs.microsoft.com/en-us/azure/virtual-machines/windows/quick-create-portal](https://docs.microsoft.com/en-us/azure/virtual-machines/windows/quick-create-portal)

## Community

If you have any questions, feel free to reach out to us at devrelations@symbl.ai or through our [Community Slack](https://join.slack.com/t/symbldotai/shared_invite/zt-4sic2s11-D3x496pll8UHSJ89cm78CA) or our [forum](https://community.symbl.ai/?_ga=2.134156042.526040298.1609788827-1505817196.1609788827).

This guide is actively developed, and we love to hear from you! Please feel free to [create an issue](https://github.com/SymblDev/symbl-livekit-rtc-app/issues) or [open a pull request](https://github.com/SymblDev/symbl-livekit-rtc-app/pulls) with your questions, comments, suggestions, and feedback. If you liked our integration guide, please star our repo!

## License

This library is released under the [MIT](https://github.com/SymblDev/symbl-livekit-rtc-app/blob/master/LICENSE.txt)
