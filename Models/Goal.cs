using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soulbound.Models
{
    class Goal
    {
        public string Id { get; set; } = String.Empty;
        public string Title { get; set; } = String.Empty;     // описание цели
        public string TimeToComplete { get; set; } = String.Empty;              // например, количество дней

        // Категории
        public bool IsPhysical { get; set; } = false;
        public bool IsMental { get; set; } = false;
        public bool IsIntellectual { get; set; } = false;

        //Выполнена или нет
        public bool IsCompleted { get; set; } = false;

        // Дополнительно можно добавить дату создания или статус выполнения
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
