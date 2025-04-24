import { FC, memo } from "react";
import Stock from "../../interface/Stock";
import PriceTrackerCell from "./PriceTrackerCell";
import PriceTrackerRowButtonCell from "./PriceTrackerButtonCell";
import PriceTrackerPriceCell from "./PriceTrackerPriceCell";
import PriceTrackerUpdatedAtCell from "./PriceTrackerUpdatedAtCell";

interface PriceTrackerRowProps{
  stock:Stock;
  isSubscribed: boolean;
  onSubscribe:(id:number)=>void;
}
/**
 * This component could have been sufficient for our use case and we could have rendered the whole row on message receive, 
however, went ahead and further divide td element to its own component, this is to showcase and avoid rending other td element like ID and td containing the subscribe button. 
This might not be more useful in this app, but in big Trading/Analytical platforms, where we have so many columns in the row and cell in the row could contain other visual elements like icon/charts/RAG status/action buttons etc.  
Rendering only the cell has changes will give user better experience and save resources.   
 */
const PriceTrackerRow:FC<PriceTrackerRowProps>  = memo(({stock, isSubscribed, onSubscribe})=>{
  return(
  <tr >
    <PriceTrackerCell value={stock.id}></PriceTrackerCell>
    <PriceTrackerCell value={stock.name}></PriceTrackerCell>
    <PriceTrackerPriceCell price={stock.price}></PriceTrackerPriceCell>
    <PriceTrackerUpdatedAtCell updatedAt={stock.updatedat}></PriceTrackerUpdatedAtCell>
    <PriceTrackerRowButtonCell id={stock.id} isSubscribed={isSubscribed} onClick={onSubscribe}></PriceTrackerRowButtonCell>
</tr>
);
});

export default PriceTrackerRow;