import { FC, useCallback, useState } from "react";
import STOCK_DATA from "../../constants";
import Stock from "../../interface/Stock";
import PriceTrackerRow from "./PriceTrackerRow";
import useWebSocket from "../../hooks/useWebsocket";

const PriceTracker:FC = ()=>{
    const url:string='ws://localhost:5000/ws/stock'; // this will come from some config file or enviornment variable.
    // please run .net webapi on 5000 port in order to work, quick instruction/comand to run web API using terminal is in readme file.
    const [stockList, setStockList] = useState<Stock[]>(STOCK_DATA);
    const [subscribedList, setSubscribedList] = useState<Set<number>>(new Set());  

    const { sendMessage } = useWebSocket(url,(message)=>{
        try {
            const incomingMessaage = JSON.parse(message.data);
            /* When this application grows incoming and outgoing message payload also grows/ in real world trading app, 
            we could compress this message data to reduced latency and bandwidth consumption and less stress on network.
            Long heavy Json could compress into little compressed version, UI and server both have to support the compression, in order to work.
            For this app it is ok.          
            if the data below more complex and needed some kind of transformation before providing it to react state, 
            generator pattern can be useful and also it will help throttling the update to ui and improve performance when server send data too frequently then needed.                                   
            */
            setStockList((prevStockObj) => {
            let isNewPrice = false;
            const newStockObj = prevStockObj.map((stock) => {
                if (parseInt(incomingMessaage.Id) === stock.id && parseInt(incomingMessaage.Price) !== stock.price) {          
                    isNewPrice = true;
                    const temp = { ...stock, price: incomingMessaage.Price, updatedat: new Date(incomingMessaage.UpdatedAt) };
                    return temp;
                }
            return stock;
            });
            return isNewPrice ? newStockObj : prevStockObj;
        });
        } catch (error) {
        console.error('WebSocket(OnMessage), Error:', error);
        }   
    });

    const onSubscribe = useCallback((id:number)=>{
        setSubscribedList((pre)=>{
        const slist = new Set(pre);
        if (slist.has(id)) {
            slist.delete(id);
            const mesage = 'unsubscribe:'+id;   
        /*  When this application grows incoming and outgoing message payload also grows/ in real world trading app, 
            we could compress this message data to reduced latency and bandwidth consumption and less stress on network.
            For this app it is ok */
            sendMessage(mesage);
            
        } else {
            slist.add(id);
            const mesage = 'subscribe:'+id;
            /*When this application grows incoming and outgoing message payload also grows/ in real world trading app, 
            we could compress this message data to reduced latency and bandwidth consumption and less stress on network.
            For this app it is ok */              
            sendMessage(mesage);
        }
        return slist;
        });
    },[sendMessage]);

    return(
        <div>
            <table>
            <thead >
               <tr>
                <th >ID</th>
                <th >Name</th>
                <th >Price</th>
                <th >Updated At</th>
                <th ></th>
               </tr>
            </thead>
            <tbody>
            {stockList.map((stock)=>{
            return  <PriceTrackerRow key={stock.id} stock={stock} isSubscribed={subscribedList.has(stock.id)} onSubscribe={onSubscribe}></PriceTrackerRow>
            })}
            </tbody>
          </table>  
        </div>
    );
}
export default PriceTracker;