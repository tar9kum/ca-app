import { FC, memo } from "react";

interface PriceTrackerCellProps{
  value:number |string ; 
}
/* In Memo for fine grain prop check, we could add second return function in memo to compare deep copy */
const PriceTrackerCell:FC<PriceTrackerCellProps>  = memo(({value})=>{
    return(
        <td >{value}</td>
  );
});
export default PriceTrackerCell;