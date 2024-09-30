using Microsoft.Maui.Layouts;
using System.Diagnostics;

namespace iOSTranslationArrangeBug {

    public class CAbsoluteLayout : AbsoluteLayout {
        protected override ILayoutManager CreateLayoutManager() {
            return new CustomAbsoluteLayoutManager(this);
        }

    }

    //======================
    //LAYOUT MANAGER
    //======================
    //https://github.com/dotnet/maui/blob/main/src/Core/src/Layouts/AbsoluteLayoutManager.cs#L8
    public class CustomAbsoluteLayoutManager : LayoutManager {
        public CustomAbsoluteLayoutManager(IAbsoluteLayout layout) : base(layout) {
            customAbsoluteLayout = layout;
        }
        public IAbsoluteLayout customAbsoluteLayout { get; }
        const double AutoSize = -1;

        public override Size Measure(double widthConstraint, double heightConstraint) {

            Debug.WriteLine("MEASURE CHILDREN OF: " + (customAbsoluteLayout as CAbsoluteLayout).StyleId + " NUM CHILDREN " + customAbsoluteLayout.Count);

            var availableWidth = widthConstraint;// - padding.HorizontalThickness;
            var availableHeight = heightConstraint;// - padding.VerticalThickness;

            double measuredHeight = 0;
            double measuredWidth = 0;

            for (int n = 0; n < customAbsoluteLayout.Count; n++) {
                var child = customAbsoluteLayout[n];

                if (child.Visibility == Visibility.Collapsed) {
                    continue;
                }

                var bounds = customAbsoluteLayout.GetLayoutBounds(child);
                
                bool isWidthProportional = false;
                bool isHeightProportional = false;

                var measureWidth = ResolveChildMeasureConstraint(bounds.Width, isWidthProportional, widthConstraint);
                var measureHeight = ResolveChildMeasureConstraint(bounds.Height, isHeightProportional, heightConstraint);

                var measure = child.Measure(measureWidth, measureHeight);

                var width = ResolveDimension(isWidthProportional, bounds.Width, availableWidth, measure.Width);
                var height = ResolveDimension(isHeightProportional, bounds.Height, availableHeight, measure.Height);

                measuredHeight = Math.Max(measuredHeight, bounds.Top + height);
                measuredWidth = Math.Max(measuredWidth, bounds.Left + width);
            }

            var finalHeight = ResolveConstraints(heightConstraint, customAbsoluteLayout.Height, measuredHeight, customAbsoluteLayout.MinimumHeight, customAbsoluteLayout.MaximumHeight);
            var finalWidth = ResolveConstraints(widthConstraint, customAbsoluteLayout.Width, measuredWidth, customAbsoluteLayout.MinimumWidth, customAbsoluteLayout.MaximumWidth);

            return new Size(finalWidth, finalHeight);
        }
        
        public override Size ArrangeChildren(Rect bounds) {

            Debug.WriteLine("ARRANGE CHILDREN OF: " + (customAbsoluteLayout as CAbsoluteLayout).StyleId + " NUM CHILDREN " + customAbsoluteLayout.Count);
            var padding = customAbsoluteLayout.Padding;

            double top = padding.Top + bounds.Top;
            double left = padding.Left + bounds.Left;
            double availableWidth = bounds.Width - padding.HorizontalThickness;
            double availableHeight = bounds.Height - padding.VerticalThickness;

            for (int n = 0; n < customAbsoluteLayout.Count; n++) {
                IView child = customAbsoluteLayout[n];

                if (child.Visibility == Visibility.Collapsed) {
                    string invisibleID = "";
                    if (child as VisualElement != null) {
                        invisibleID = (child as VisualElement).StyleId;
                    }
                    continue;
                }

                var destination = customAbsoluteLayout.GetLayoutBounds(child);
                
                bool isWidthProportional = false;
                bool isHeightProportional = false;

                destination.Width = ResolveDimension(isWidthProportional, destination.Width, availableWidth, child.DesiredSize.Width);
                destination.Height = ResolveDimension(isHeightProportional, destination.Height, availableHeight, child.DesiredSize.Height);

                destination.X += left;
                destination.Y += top;

                child.Arrange(destination);
            }

            return new Size(availableWidth, availableHeight);
        }

        static bool HasFlag(AbsoluteLayoutFlags a, AbsoluteLayoutFlags b) {
            // Avoiding Enum.HasFlag here for performance reasons; we don't need the type check
            return (a & b) == b;
        }

        static double ResolveDimension(bool isProportional, double fromBounds, double available, double measured) {
            // By default, we use the absolute value from LayoutBounds
            var value = fromBounds;

            if (isProportional && !double.IsInfinity(available)) {
                // If this dimension is marked proportional, then the value is a percentage of the available space
                // Multiple it by the available space to figure out the final value
                value *= available;
            }
            else if (value == AutoSize) {
                // No absolute or proportional value specified, so we use the measured value
                value = measured;
            }

            return value;
        }

        static double ResolveChildMeasureConstraint(double boundsValue, bool proportional, double constraint) {
            if (boundsValue < 0) {
                // If the child view doesn't have bounds set by the AbsoluteLayout, then we'll let it auto-size
                return double.PositiveInfinity;
            }

            if (proportional) {
                return boundsValue * constraint;
            }

            return boundsValue;
        }
    }
}
