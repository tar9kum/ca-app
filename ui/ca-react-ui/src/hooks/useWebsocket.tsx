import { useCallback, useEffect, useRef } from "react";
/*
    This hook is to encapsulate / extract WS conection and  lifeacyle management logic for reusability, 
    we can further enhance this hook to handle reconnection logic and connection management. Due time constraint could able to address it :( 
  
    url: web socket url to connect to server 
    OnMessageHandler : function exposed for the client to perform operation when message is received
*/
const useWebSocket = (url: string, onMessageHandler: (event: MessageEvent) => void,) => {
    const wsRef = useRef<WebSocket | null>(null);
    const connect = useCallback(() => {
    const ws = new WebSocket(url);
    wsRef.current = ws;

      ws.onopen = () => {
        console.log('Connected to WebSocket server');
        /*
            TODO(Tarun): we perform any other task on successful connection i.e.
            Authentication , WS reconnection logic to handle consistent WS connection etc.
         */
      };

      ws.onmessage = onMessageHandler;
  
      ws.onclose = () => {
        console.log(' WebSocket connection closed');     
        /*
            TODO(Tarun): 
            Manage reconnection logic and reconnection attempt, it is crucial for better user experience. 
            On connection failure, we could try connecting after regular interval (may be increase the interval time on every connection failure attempt),
            may be show the error to the user gracefully after certain number of attempt(depend on the requirement). 
            Max no of attempt is also important to avoid using unnecessary resources on the browser and hamering the server especially,
            when you server handle huge number of connections concurrently.            
            Also look into fall back option in critical systems, where timely data is parmount and lead to financial losses. 
            May look into SSE, long polling.
        */

      };
  
      ws.onerror = (error) => {
        console.error('WebSocket error:', error);
         /*
            TODO(Tarun):             
            Error handling , logging and again reconnection logic
        */
        
      };
    }, [url, onMessageHandler]);
  
    /*                    
        This manages the lifecycle of the ws connection, performing clean up and closing the connection gracefully, preventing any memory leak.
    */
    useEffect(() => {
      connect();
      return () => {
        if (
          wsRef.current &&
          (wsRef.current.readyState === WebSocket.OPEN ||
            wsRef.current.readyState === WebSocket.CONNECTING)
        ) {
          wsRef.current.close();
          console.log(' WebSocket clean up.');  
        }
      };   
}, []);
  
    /*                    
        client can use this to send message to server, utilising the same connection.
    */
    const sendMessage = useCallback((message: string) => {
        if (wsRef.current && wsRef.current.readyState === WebSocket.OPEN) {
          wsRef.current.send(message);
        }
      }, []);
  
    return { sendMessage };
  };
  export default useWebSocket;