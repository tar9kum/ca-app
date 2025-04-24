## ca-app

# CA.WEB.API

```js
Quickest way to run the ws api
Clone the repo.
Open the folder CA.WEB.API where csproj project file reside.
Right-click the folder and open visual studio.
Click on the top menu "Build" and Build the project.
Click on the top menu "View" and click Terminal.
In your developer PowerShell Terminal, run the command dotnet run.
It should start our API exposing our ws endpoint.
Please run on the port 5000(else you can change the port in the UI) to make it insync with the UI.
Now .net api WS has started listening on port 5000 "ws://localhost:5000/ws/stock"
```


# Possible Improvement 

1. TLS needs to be configured, not utilising transport level security SSL Cert.
2. WebSocket connection need to be scalable and distributed across multiple node/server for real world trading/analytic application.
3. We need to think about backpressure mechanisms throttling/debouncing. A generator pattern can be handy.
4. More efforts are to enhance connection management, monitoring,  heartbeat, etc.
5. Data compression is also something important when message(s) become heavy, to reduce the bandwidth and network load. we are already sending delta change to ui in this app(not the whole stock object).
6. Efforts are required to decouple connection logic with subscription logic (distributed cache had come in handy)
7. Also left some improvement comments on the code itself while coding.
8. In WebSocketManager, we could look into further optimising subscription management, may be instead of storing subscriptions per connection, store connections per stock. we need to the measure it first before optimising(it should be faster).
9. This solution is good for small to medium loads, but for real-world high through-put systems this won't be enough, To achieve high scalability   decoupling StockPriveUpdateService with WebSocketManager is required (message-broker/queue could be used).
10. More effort is required to improve error handling and ws connection and lifecycle management.

I could have address some of the point above, due to time constraint, could not able to do that.  
Thank you for your time.

# ca-react-ui

```js
Clone the repo.
Open the folder ca-react-ui where package.json file reside.
Right-click the folder and open visual code.
Click on the top menu "View" and click Terminal.
In your developer PowerShell Terminal, run command "npm i".
It should create node_modules and will install dependencies.
After install, run "npm run dev" 
Make sure UI port is the same as the server (default 5000)
```


# Possible improvement

1.  useWebsocket() custom hook.
    Reconnection logic is essential for a better user experience. The useWebsocket hook should automatically try reconnecting at a regular time interval on connection failure. Maybe increase the time interval on each failed attempt. Set the max-try count as well, so after that, we could gracefully handle the error and notify the user without hammering the server and wasting browser and server resources (exponential backoff).
    
    Expose connection status from the hook to the client/component.

    Depending on the requirement we could think of buffering/throttling/debounce techniques to manage messages. 
2. Though we don't need for this solution, In real-world distributed applications, useState will not be enough to manage application state, for complex state management, we could look into react context API and/or state management lib like redux.
3. Maybe PriceTrackerRow component would have been enough for this use case and we might not need a cell-based component. I did it to showcase, In real real-world app especially an analytical/trading platform, when cell of the table can have nested visual elements like button/rag/icon/tooltips/other-component, etc and the grid can have so many columns, rendering the whole row might not be a good idea. This approach will use less resources and better user experience.
4. If the server sending update more the UI should/can process, we could use queue and process the data at consistent rate/some backpressure technique.
5. Data compression could be a good option, when message size increases, the server should support it as well.
6. I left a similar comment on the code as well, due to time constraints this is what I came up with, Hope this all makes sense : )

Thank you for your time.



