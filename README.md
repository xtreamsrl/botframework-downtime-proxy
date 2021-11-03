# Bot Framework Downtime Proxy
An Azure deployable Function that can be used in Azure Channel Registration as endpoint and redirects inbound traffic to a downstream Bot Framework engine (that can run as an App Service instance, for example). This is particularly useful for experimental and R&D bots that cannot afford a reliable production configuration for the bot, yet they still need a way to notify users when the bot is temporarily down - which happens a lot for free App Service plans. 

## Configuration
The following env variables must be provided to make the proxy works properly. 
Some of them are optional with a sensible default.

- **MessagingEndpoint** \
   The url where the messages, sent to your bot, will arrive.
- **MicrosoftAppId** and **MicrosoftAppPassword** \
   Credentials used by the bot connector to authenticate the calls to your Bot's service.     
- **TimeoutInSeconds** (optional) \
   Time to await before sending a courtesy message to the user if the bot has not answered 
   already. A default value of 5 is provided.
- **WaitFeedbackReply** (optional) \
   The courtesy message to send to the user. A default one is provided.

# Who we are
<img align="left" width="80" height="80" src="https://avatars2.githubusercontent.com/u/38501645?s=450&u=1eb7348ca81f5cd27ce9c02e689f518d903852b1&v=4">
A proudly 🇮🇹 software development and data science startup.<br>We consider ourselves a family of talented and passionate people building their own products and powerful solutions for our clients. Get to know us more on <a target="_blank" href="https://xtreamers.io">xtreamers.io</a> or follow us on <a target="_blank" href="https://it.linkedin.com/company/xtream-srl">LinkedIn</a>.
