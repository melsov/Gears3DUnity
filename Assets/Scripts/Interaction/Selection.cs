using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class Selection : Singleton<Selection>
{
    protected Selection() { }

    private Cog _selected;
    public Cog selected {
        private get {
            return _selected;
        }
        set {
            highlight(false);
            _selected = value;
            highlight(true);
        }
    }

    private void highlight(bool doHighlight) {
        if (!_selected) { return; }

        
        
    }

}

