# RicsMagicMirror
My backend code for serving a PNG file which can be displayed by a (rooted) Kindle v4

Kindle Rooting instructions etc I used were found here https://matthealy.com/kindle but instead of using the suggested approach of taking a screenshot of a webpage this project is a web enpoint that generates a PNG directly from the data sources. It provides Train departures, Bus departures, Weather and Google calendar events. It is very bespoke to my own needs and I'm not expecting to make it modular etc. It's all just here as an example.

Note the mostly empty Constants.cs file, which is where I stashed all my private keys, then blanked them out before push-ing

The magic happens in Pages/GenerateController.cs - invoked via /api/generate - you can also add ?force=true to bypass the caching

TODO:
 - Change the caching strategy to save "last fetched" timestamps against each of the APIs, at the minute it stronly relies on the fact that it is invoked once a minute (so, for example if you call it at 2:59pm and then 3:01pm it won't refresh the calendar, which only happens on the hour)

