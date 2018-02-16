<img src="https://github.com/neilpimley/McKenziesPharmacy-ui/raw/master/src/assets/images/cross.png" height="30" /> McKenzies Pharmacy - Prescription Re-ordering Service
================================

This repository contains the source code of the backoffice application for a sample Repeat Prescription Re-ordering Application designed for McKenzies Pharmacy and used by myself to try out new technologies.

![alt text](https://github.com/neilpimley/McKenziesPharmacy-dispensing/blob/master/screenshot.PNG)

## System Components

- Web Application (.NET Core 2.0 / MVC / Razor, Azure AD)
- Core Services ( .NET Core 2.0, EF Core - services / respositories / models)
- Database (DB deployment project)

## Dependencies 
- Packages https://www.myget.org/F/mckenziespharmacy/api/v3/index.json

## Related Source Code
- https://github.com/neilpimley/McKenziesPharmacy-core



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



