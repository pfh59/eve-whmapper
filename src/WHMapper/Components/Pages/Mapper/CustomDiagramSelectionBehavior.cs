using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Events;
using Blazor.Diagrams.Core.Models.Base;

namespace WHMapper.Components.Pages.Mapper
{
	public class CustomDiagramSelectionBehavior : Behavior
    {
		public CustomDiagramSelectionBehavior(Diagram diagram) : base(diagram)
        {

            Diagram.PointerDown += OnPointerDown;
        }


        private void OnPointerDown(Model? model, PointerEventArgs e)
        {
            if (e.Button==0)
            {
                if (model == null)
                {
                    Diagram.UnselectAll();
                }
                else if (model is SelectableModel sm)
                {
                    if (e.CtrlKey && sm.Selected)
                    {
                        Diagram.UnselectModel(sm);
                    }
                    else if (!sm.Selected)
                    {
                        Diagram.SelectModel(sm, !e.CtrlKey || !Diagram.Options.AllowMultiSelection);
                    }
                }
            }
        }
        
        public override void Dispose()
        {
            Diagram.PointerDown -= OnPointerDown;
        }
    }
}

