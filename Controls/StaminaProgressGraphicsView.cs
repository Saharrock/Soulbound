using Microsoft.Maui.Controls;

using Microsoft.Maui.Graphics;



namespace Soulbound.Controls;



// Кастомная полоска stamina на MainRoomPage. Progress = 0..1 от WeeklyStaminaCap

public sealed class StaminaProgressGraphicsView : GraphicsView

{

    public static readonly BindableProperty ProgressProperty = BindableProperty.Create(

        nameof(Progress),

        typeof(double),

        typeof(StaminaProgressGraphicsView),

        0.0,

        propertyChanged: (bindable, _, _) => ((StaminaProgressGraphicsView)bindable).Invalidate());



    public double Progress

    {

        get => (double)GetValue(ProgressProperty);

        set => SetValue(ProgressProperty, value);

    }



    public StaminaProgressGraphicsView()

    {

        HeightRequest = 16;

        Drawable = new StaminaProgressDrawable(() => Progress);

    }



    private sealed class StaminaProgressDrawable : IDrawable

    {

        private readonly Func<double> getProgress;



        public StaminaProgressDrawable(Func<double> getProgress)

        {

            this.getProgress = getProgress;

        }



        public void Draw(ICanvas canvas, RectF dirtyRect)

        {

            double p = Math.Clamp(getProgress(), 0.0, 1.0);

            float x = dirtyRect.X;

            float y = dirtyRect.Y;

            float w = dirtyRect.Width;

            float h = dirtyRect.Height;

            float corner = Math.Min(h * 0.35f, 8f);



            canvas.Antialias = true;



            // Тёмный фон — пустая часть пула

            canvas.SetFillPaint(new SolidPaint(Color.FromArgb("#5D2F18")), dirtyRect);

            canvas.FillRoundedRectangle(x, y, w, h, corner);



            // Зелёная заливка — оставшаяся stamina

            float fillW = (float)(w * p);

            if (fillW > 0.5f)

            {

                canvas.SetFillPaint(new SolidPaint(Color.FromArgb("#3B9E4E")), dirtyRect);

                canvas.FillRoundedRectangle(x, y, fillW, h, corner);

            }

        }

    }

}


