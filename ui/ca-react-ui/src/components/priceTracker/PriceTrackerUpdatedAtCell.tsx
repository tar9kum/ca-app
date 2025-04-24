import { FC, memo } from "react";

interface PriceTrackerUpdatedAtCellProps{
  updatedAt:Date;
}
const PriceTrackerUpdatedAtCell:FC<PriceTrackerUpdatedAtCellProps>  = memo(({updatedAt})=>{   
    return(
        <td >{updatedAt.toLocaleString()}</td>
  );
});
export default PriceTrackerUpdatedAtCell;