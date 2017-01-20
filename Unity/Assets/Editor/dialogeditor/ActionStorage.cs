using System.Collections.Generic;
using UnityEngine;

namespace Assets.Dia
{
    public class ActionStorage
    {
        public static BaseDialogNode ClickedNode;
        public static Rect ClickLocation;
        public static int ClickedIndex=-1;
        public static List<BaseDialogNode> Windows=new List<BaseDialogNode>();
    }
}