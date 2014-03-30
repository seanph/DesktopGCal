# DesktopGCal

A desktop calendar built to sync with Google Calendar - a WPF application developed using C#.
My first project on GitHub so if this breaks, sets things on fire and disappears, don't be surprised!

## Notes
Currently a work in progress, and the user interface is not laid out for end-users, but for testing.

## Additional setup
This project requires access to the Google Calendar API, and authentication is performed using OAuth v2. As such, a client_secrets file is required:
* Go to [Google's developer center](https://developers.google.com/google-apps/calendar/) and create a project that uses the Calendar API. 
* Go to the developer console and download a Client Secret for native applications. 
You will receive a "client_secrets.json" file, which should be placed in the same directory as the DesktopGCal executable. You're now good to go!

Released under the BSD license - see LICENSE.md for more details.
Copyright (c) 2014 Sean Phillips