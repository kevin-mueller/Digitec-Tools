# Shopping-Tools
![.NET Build](https://github.com/kevin-mueller/Shopping-Tools/workflows/.NET%20Build/badge.svg?branch=master)
![.NET Test](https://github.com/kevin-mueller/Shopping-Tools/workflows/.NET%20Test/badge.svg?branch=master)

Allows you to do stuff with online shops, that they don't want you to.
This repository also contains Api's for various online shops.

Use the Shopping-Tools-Api-Services.dll as an interface to the online shops. Currently the Api's are very limited. Fell free to contribute. :)


In order to build and run this on your own server, install the Admin SDK credentials json file from your Firebase Console.
https://console.firebase.google.com/u/0/project/your-project-name/settings/serviceaccounts/adminsdk

Click on Generate New Private Key
Save the file somewhere LOCALLY. Don't publish it!

These are all the Environment Variables you need to set:

SHOPPINGTOOLSCONNECTIONSTRING={Connectionstring To the Identity Database}

DIGITEC_TOOLS_GMAIL_CREDENTIALS={Path To The GMAIL SMTP Credentials File} (consists of user={email} \n and password={password}

SendGridUser={User Name for SendGrid}

SendGridKey={Key From SendGrid}

GOOGLE_APPLICATION_CREDENTIALS={Path To The Private Key You Just Generated}

SCRAPERAPIKEY={Key from scraperapi.com}


Windows: Use the GUI. Everything else is buggy as hell. (Login and out afterwards)
If hosting on Linux, write it directly to the service config file.

The Identity System needs a running MySQL Server.
On Windows use the installer: https://dev.mysql.com/downloads/windows/installer/8.0.html
On Linux: https://raspberry-projects.com/pi/software_utilities/web-servers/mysql

Create a new User afterwards and use it in the connection string.

# Please Consider Supporting
<a href="https://www.buymeacoffee.com/KevinMueller" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-orange.png" alt="Buy Me A Coffee" style="height: 51px !important;width: 217px !important;" ></a>
