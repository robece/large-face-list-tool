Large face list tool, is a console application created to facilitate the Large Face List manipulation.

<div style="text-align:center">
    <img src="http://rcervantes.me/images/large-face-list-tool-console.png" width="800" />
</div>
<br/>

## Tasks can be performed

- Add large face lists
- Delete large face lists
- Add faces in a large face list (provides Telemetry in App Insights)
- Train large face list (provides Telemetry in App Insights)
- Find similar faces in large face lists (provides Telemetry in App Insights)

## Setup the project

#### Clone the project from GitHub repo

`git clone https://github.com/rcervantes/large-face-list-tool.git`

## Before compile

Edit the file Settings.cs with the correct values:

- AzureWebJobsStorage: "AZURE_STORAGE_CONNECTION_STRING"
- FaceAPIKey : "FACE_API_KEY"
- Zone: "FACE_API_ZONE(e.g. westus or southcentralus)"
- AppInsightsKey: "AZURE_APPINSIGHTS_INSTRUMENTATION_KEY"

Copy sample\Workspace folder from the repo to C:\Workspace\

Workspace folder contains:

- Images folder: "Folder used to allocate all images for a massive manipulation"
- Metadata folder: "Folder used to allocate all json files for a massive manipulation"
- FindSimilar folder: "Folder used to allocate the input image that will be find in all large face lists"

#### Important: The name of the image must be the same name of the json file, the json file must contain a "Unique Business Identificator" for the face.

## Running the console app

Now it's time to create or use an existing large face list.

In the top of the console there are some settings needed:

Settings required:
- LargeFaceListId: PENDING CONFIGURATION!
- ImageFolderPath: PENDING CONFIGURATION!
- MetadataFolderPath: PENDING CONFIGURATION!
- FindSimilarFolderPath: PENDING CONFIGURATION!

You can configure this with options:

- [ 1 ] Create large face list
- [ 2 ] Assign large face list
- [ 5 ] Set ImageFolderPath, MetadataFolderPath and FindSimilarFolderPath for training

Then you can manipulate the large face list using the options: 

- [ 6 ] Add faces to large face list
- [ 7 ] Train large face list
- [ 8 ] Find similar faces in all large lists

## Telemetry

Remember that you can access to App Insigths -> Search to verify the activity and traces for the following operations:

- [ 6 ] Add faces to large face list
- [ 7 ] Train large face list
- [ 8 ] Find similar faces in all large lists

<div style="text-align:center">
    <img src="http://rcervantes.me/images/large-face-list-tool-telemetry1.png" />
</div>
<br/>

<div style="text-align:center">
    <img src="http://rcervantes.me/images/large-face-list-tool-telemetry2.png" />
</div>
<br/>

<div style="text-align:center">
    <img src="http://rcervantes.me/images/large-face-list-tool-telemetry3.png" />
</div>

