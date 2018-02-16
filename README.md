<a href="http://mckenziespharmacy.azurewebsites.net/">
    <h2>
        <img src="https://github.com/neilpimley/McKenziesPharmacy-ui/raw/master/src/assets/images/cross.png" height="20" />
    McKenzies Pharmacy</h2>
</a>

Prescription Re-ordering Service - Dispensing Application
=========================================================

Back Office application element for Prescription re-ordering application

![alt text](https://github.com/neilpimley/McKenziesPharmacy-dispensing/blob/master/screenshot.PNG)


This repository contains the source code of back office element for a sample Repeat Prescription Re-ordering Application designed for McKenzies Pharmacy and used by myself to try out new technologies

References core components in following NuGet package
https://github.com/neilpimley/McKenziesPharmacy-core


## System Components

- Web Application (.NET Core 2.0 / MVC / Razor, Azure AD)
- Core Services ( .NET Core 2.0, EF Core - services / respositories / models)
- Database (DB deployment project)

## Applicatoin settings required in azurewebsites

In the local dev environment use "manage user secrets" in Visual Studio and add the following to secrets.json
```json
{
    "ConnectionStrings": {
        "Entities": ""
    },
    "ServiceSettings": {
        "SendGridApiKey": "",
        "GetAddressApiKey": "",
    }
}
```

If hosted in Azure add the application settings below
```
ServiceSettings:SendGridApiKey
ServiceSettings:GetAddressApiKey
```



