import { FC, MouseEvent, memo } from "react";
import styles from "../../css/PriceTracker.module.css" ;

interface PriceTrackerRowButtonCellProps{
  id:number;
  onClick:(id:number)=>void;
  isSubscribed:boolean;
} 
/* prop drilling in our case is ok, not a overhead. for more complex state management scenario,
   we could look into state management libraries like redux;
  In Memo for fine grain prop check, we could add second return function in memo to compare deep copy */
const PriceTrackerRowButtonCell:FC<PriceTrackerRowButtonCellProps>  = memo(({id, isSubscribed, onClick})=>{
 
  const handleSubscribe = (event: MouseEvent<HTMLButtonElement>)=> {
    const dId = event.currentTarget.dataset.id || '0';
    onClick(parseInt(dId));
  }
    return(
        <td >  
          <button data-id={id} 
        className={isSubscribed ?  styles.subscribed : ""}
        onClick={handleSubscribe}>{isSubscribed? "UnSubscribe": "Subscribe"}
        </button>
        </td>
  );
});

export default PriceTrackerRowButtonCell;


