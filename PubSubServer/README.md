# PubSubServer

# Set env variables

## Redis (mandatory)

We use redis as our underlying messagebroker. Add a message to redis and the pubsub server will deliver it by websocket.

REDIS_CONNECTION_STRING=your redis connection string
REDIS_PUBSUB_CHANNEL=redis
DISABLE_AUTHENTICATION=false

## Database for Protection Keys

This is asp net core specific. At the moment only mssql is supported.

APPLICATION_NAME=My Application
USE_DATABASE=true
USE_DATA_PROTECTION=true
USE_MSSQL=true
MSSQL_CONNECTION_STRING=my connection string

## Authentication (general)

If you need any authentication you need to enable it.

USE_AUTHENTICATION=true

## Cookie Authentication (optional)

AUTHENTICATION_SCHEME=Cookies
USE_COOKIE_AUTHENTICATION=true
SLIDING_EXPIRATION = true

None=0 Lax=1 Strict=2
SAME_SITE=1 

HTTP_ONLY=true

SameAsRequest=0 Always=1 None=2
SECURE_POLICY=1
EXPIRE_TIME_SPAN=1:00:00
COOKIE_NAME=.myCookie
DOMAIN=example.com
PATH=/


## JWT Authentication (optional) implementation is in progress right now

JWT_ISSUER=yourdomain.com
JWT_AUDIENCE=yourdomain.com
JWT_KEY=your super secret shared key

# Client Setup

Add the SignalR Script -> At the moment this compilation uses v6.0.23
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/6.0.23/signalr.min.js"></script>

Use this script for the client

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/pubsub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
        // now we can use the connection
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
};

connection.onclose(async () => {
    await start();
});

// setup your handlers
connection.on("Subscribed", (topic) => {
    console.log(topic);
});

// Start the connection.
start();

Look at https://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client?view=aspnetcore-6.0&tabs=visual-studio for more details.

# Subscrube and Unsubscribe

await connection.invoke("subscribe", "topic", true);

await connection.invoke("unsubscribe", "topic", true);

The last parameter is used if you want to broadcast a subscribe action. A client subscribed to topic X -> all others will know.