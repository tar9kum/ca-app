import { FC, memo, useEffect, useRef, useState } from "react";
import styles from "../../css/PriceTracker.module.css" ;

// we could move into separate interface Props file, for this app it is ok.
interface PriceTrackerPriceCellProps{
  price:number ;
}
/* In Memo for fine grain prop check, we could add second return function in memo to compare deep copy */
const PriceTrackerPriceCell:FC<PriceTrackerPriceCellProps>  = memo(({price})=>{
const [priceIndicatorValue, setPriceIndicatorValue] =  useState<number |null>(null);
const indicator = useRef<boolean| null>(null);
  useEffect(() => {   
    if(priceIndicatorValue){
      indicator.current=
        price > priceIndicatorValue ? true :
        price < priceIndicatorValue ? false :
        null;      
    }
    else
      indicator.current=null;
  
    setPriceIndicatorValue(price);
  }, [price]);

    return(
        <td className={ indicator.current!== null ?   
                          indicator.current === true? 
                              styles.priceUp:
                              styles.priceDown : 
                        styles.priceSame}>
            {price.toFixed(2)}
          </td>
  );
});
export default PriceTrackerPriceCell;
