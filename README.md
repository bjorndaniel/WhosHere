# Who's Here - ML assisted meeting attendance tracking

This is an app (or apps rather) I built during a News from Build 2019 session at work, aiming to show of different new and semi-new features from Microsoft. 

Using a mobile app to take a photo of the people in attendance it will be analyzed and matched against images from your companys Azure AD. The identified people will then be shown on a website.

Built using .NET Core 3 preview 5

## The most important Nuget packages

* [Microsoft.Graph](https://github.com/microsoftgraph/msgraph-sdk-dotnet) - Get images and email-addresses from your Azure AD
* [Microsoft.Identity.Client](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/) -  Authenticate against your Azure AD
* [Microsoft.Storage.Blob](https://www.nuget.org/packages/Microsoft.Azure.Storage.Blob/) - Store images for easy access from the website
* [Microsoft.Azure.CognitiveServices.Vision.Face](https://github.com/Azure/azure-sdk-for-net) - The library used to identify faces
* [Microsoft.Azure.WebJobs.Extensions.SignalRService](Microsoft.Azure.WebJobs.Extensions.SignalRService) - Azure function bindings for the [Azure SignalR Service](https://azure.microsoft.com/en-us/services/signalr-service/)
* [Microsoft.Extensions](https://github.com/aspnet/Extensions) - Used to make sure no secrets are checked in to source control, handle configuration and dependency injection
* [Xam.Plugin.Media](https://github.com/jamesmontemagno/MediaPlugin) - To access the camera on iOS and Android
* [Blazor.Extensions.SignalR](Blazor.Extensions.SignalR) - Handle the SignalR connection in the Blazor WASM site

## The solution exists of these projects

* **WhosHere.Common -**
 A .NET standard project used to connect to the various apis
* **WhosHere.Functions -**
Azure functions handling the SignalR service and images to be analyzed
* **WhosHere.Mobile -**
A Xamarin Forms app to capture photos and send them to the Azure function to be analyzed
* **WhosHere.Wpf -**
A .NET Core Wpf application used to connect to the Microsoft graph, fetch employee images and send them to Azure Blob Storage and the face api. Also has a tab for uploading test images to be analyzed.
* **<span>WhosHere</span>.Web -**
A Blazor WASM app that shows the state of the employees and moves them from there to here as the analyze results come in over SignalR.

## How to build and run

**1.** Clone the project

**2.** Run `dotnet restore`

**3.** cd into `WhosHere.Wpf`

**4.** Run `dotnet user-secrets init` This will create a `<UserSecretsId>` tag in the .csproj-file containing a GUID.

**5.** Run `dotnet user-secrets set "ConfigValues:Tenant" "common"` This will create a secrets.json file in your home folder `C:\Users\YOUR_USERNAME\AppData\Roaming\Microsoft\UserSecrets\GUID_GENERATED_BY_THE_CLI`

**6.** Open the secrets.json file and copy all values from appsettings.json so that they can be used to set the various keys needed later.

**7.** Copy the `<UserSecretsId>` to the `WhosHere.Functions.csproj` file

**8.** Open the [Azure Portal](https://portal.azure.com)

**9.** Create a new [App Registration](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal) and put the Application ID into ClientID in secrets.json **Make sure you don't put any keys into appsettings.json**

**10.** Make sure the app has permissions User.Read.All and User.Read in the Microsoft Graph, ID tokens is checked, the redirect uri is https://login.microsoftonline.com/common/oauth2/nativeclient and that Supported account types are any

**11.** Create a [Face Cognitive Service](https://azure.microsoft.com/en-us/try/cognitive-services/) and put the key into FaceApiKey

**12.** Create an [Azure SignalR Service](https://azure.microsoft.com/en-us/services/signalr-service/) and put the connectionstring into `AzureSignalRConnectionString` in local.settings.json in WhosHere.Functions

**13.** Create an [Azure Blob Storage](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-portal) and make sure the blob container is publically readable

**14.** Put account name and the container name into the `WhosHere.Web` Meeting.Razor file in this string `$"https://YOURSTORAGEACCOUNT.blob.core.windows.net/YOUR_BLOB_CONTAINER/{_}"` (line 51)

**15.** Put the accountkey into `StorageAccountKey` in `secrets.json`

**16.** Put the url (https://STORAGEACCOUNTNAME.blob.core.windows.net/CONTAINERNAME/) into `StorageAccountUrl` in `secrets.json`

**17.** Put the container name in `secrets.json` under `ContainerName`

**18.** Put the connection string in `secrets.json` under `StorageConnectionString`

**19.** In the App.xaml.cs file in `WhosHere.Mobile` set your computers ip-address followed by 7070 in the `FunctionUrl` property

**20.** You should now be ready to run the application 🤞


## Using the applications

### WhosHere.Wpf

This is the starting point, using this application you will be able to read all of the existing employee images and email-addresses from your Azure AD and put them into blob storage and the Face api

* In the `WhosHere.Wpf` folder execute `dotnet run`
* Click on the Start upload button
* There should be a prompt in the bottom of the screen telling you to go to http://microsoft.com/devicelogin.
* Open that link in a browser and paste the code (it is put in your clipboard by the app).
* Login and grant the permissions.
* Go back to the app and you should now see your images start to appear in the app.
* Once it is done you can click on Train the model
* If you'd like to try the face recognition you can switch tabs and select an image to analyze. The email-addresses of identified people will show up in the list to the right.

### WhosHere.Functions

Once you have images it is time to start the Azure functions to be able to use the Xamarin app and the website.

* In the `WhosHere.Functions` folder execute `func start`
* Once the functions startup you should see this: 

### <span>WhosHere</span>.Web

This app has a Meetingpage with a list of people and there status.

* In the `WhosHere.Web` folder execute `dotnet run`
* The app should start on localhost:5000
* Open a browser to this address and click on Meeting
* You should see the following:
* If you now look in the command line window for the functions app there should be log messages that someone has called the negotiate and employees functions.

### WhosHere.Mobile

* Connect a phone to your computer and start either the WhosHere.Android or WhosHere.iOS apps
* Go to the Photo tab and take a photo of someone you want to identify.
* The app will call the azure function and if someone is found their image will move across the screen in the Meeting page of the web app

Hopefully everything worked and you are now able to identify your co-workers 👁👍
If you have any questions or comments, file an issue and I will try to answer.

### Future ideas

* Remove all boiler plate code generated by the templates
* Implement C# 8 features, e.g. nullable reference types and async streams.
* Add a list of identified people to the mobile app
* Add an Employee card to the mobile app, if you click on a person in the list. Could be usefull if you're new at work or in a large company and manage to snap a quick photo of someone 😁
* Add the ability to send a thank you mail from the web page to those in attendance
* Send emails to those who have no profile picture
* Try to use usersecrets and appsettings in the Blazor app
