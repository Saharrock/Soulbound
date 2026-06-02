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

        propertyChanged: (bindable, _, _) => ((StaminaProgressGraphicsView)bindable).Invalidate()); //как только изменился, поменяй. Аналог OnPropertyChanged



    public double Progress

    {

        get => (double)GetValue(ProgressProperty);

        set => SetValue(ProgressProperty, value);

    }



    public StaminaProgressGraphicsView()

    {

        HeightRequest = 16; // Фиксированная высота полоски — 16 пикселей

        Drawable = new StaminaProgressDrawable(() => Progress); //Нанимаем художника

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

            double p = Math.Clamp(getProgress(), 0.0, 1.0); // Защита: если придет -0.5 или 1.5, код округлит до 0 или 1

            float x = dirtyRect.X;

            float y = dirtyRect.Y;

            float w = dirtyRect.Width;

            float h = dirtyRect.Height;

            float corner = Math.Min(h * 0.35f, 8f); // Вычисляем красивое скругление углов



            canvas.Antialias = true; // Включаем сглаживание, чтобы края полоски не были "квадратными пикселями"



            // Тёмный фон — пустая часть пула

            canvas.SetFillPaint(new SolidPaint(Color.FromArgb("#5D2F18")), dirtyRect); // Берем темно-коричневую краску

            canvas.FillRoundedRectangle(x, y, w, h, corner); // Рисуем скругленный прямоугольник на всю длину




            float fillW = (float)(w * p); // Считаем ширину зеленой полоски (общая ширина умноженная на процент прогресса)

            if (fillW > 0.5f) // Если полоска заполнена хотя бы чуть-чуть

            {

                canvas.SetFillPaint(new SolidPaint(Color.FromArgb("#3B9E4E")), dirtyRect); // Берем зеленую краску

                canvas.FillRoundedRectangle(x, y, fillW, h, corner); // Рисуем зеленую полоску поверх коричневой

            }

        }

    }

}


