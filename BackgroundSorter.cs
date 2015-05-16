using System;
using System.Drawing;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;

namespace SortingThreads
{
    /*
     * Клас, що інкапсулює все налаштування та використання aBackgroundWorker.
     * Екземпляр повинен взаємодіяти з такими сутностями:
     *  - масив цілих чисел, який потрібно відсортувати
     *  - метод сортування (у фоновому режимі)
     *  - панель вікна аплікації, яку використовують для візуалізації
     *  - об'єкт синхронізації доступу до інтерфейсу користувача (до вікна)
     *  
     * Про закінчення виконання потоку сигналізує подія SortingComplete:
     * головне вікно може реагувати на неї, змінюючи лічильники, кнопки тощо
     */
    public class BackgroundSorter
    {
        private BackgroundWorker worker;    // шаблон асинхронної взаємодії з потоком UI
        private int[] arrayToSort;          // масив і
        private SortMethod sortMethod;      //  метод для сортування
                                            // події, що впливають на відображення:
        public event SortingExchangeEventHandler SortingExchange;   // обмін двох значень
        public event SortingCompleteEventHandler SortingComplete;   // завершення сортування

        // делегат для методу сортування
        public delegate void SortMethod(int[] arrayToSort,
            BackgroundWorker worker, DoWorkEventArgs e);

        // делегати для власних подій
        public delegate void SortingExchangeEventHandler(Object sender,
            SortingExchangeEventArgs e);
        public delegate void SortingCompleteEventHandler(Object sender,
            SortingCompleteEventArgs e);

        public BackgroundSorter(int[] array, SortMethod theMethod)
        {
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            // для налаштування взаємодії потрібно задати три методи:
            // - основну довготривалу роботу
            worker.DoWork += worker_DoWork;
            // - завершальні дії після завершення основної роботи
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            // - метод інформування про хід виконання основної роботи
            worker.ProgressChanged += worker_ProgressChanged;

            this.arrayToSort = array;
            this.sortMethod = theMethod;
        }

        public BackgroundSorter(SortMethod theMethod) : this(null, theMethod)
        {
        }

        // диспетчери подій
        private void OnSortingExchange(int firstX, int secondX, int i, int j)
        {
            if (SortingExchange != null)
                SortingExchange(this, new SortingExchangeEventArgs(firstX,secondX,i,j));
        }
        private void OnSortingComplete(bool canceled)
        {
            if (SortingComplete != null)
                SortingComplete(this, new SortingCompleteEventArgs(canceled));
        }

        // задати масив поза конструктором потрібно тоді, коли ви вирішили відсортувати
        // новий масив тим самим методом у тому ж потоці
        public void SetArrayToSort(int[] array)
        {
            arrayToSort = array;
        }

        // доступ до інтерфейсу асинхронного шаблона
        public void Execute()
        {
            worker.RunWorkerAsync(arrayToSort);
        }
        public void Stop()
        {
            worker.CancelAsync();
        }

        // методи опрацювання подій асинхронного шаблона
        //   "основний цикл" - сортування
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (sortMethod != null && arrayToSort != null)
            {
                sortMethod((int[])e.Argument, sender as BackgroundWorker, e);
            }
            else throw new NullReferenceException("Trying to use null array or null sorting method");
        }
        //   на завершення перевіряєм коректність завершення,
        //   сигналізуємо про завершення
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null) MessageBox.Show(e.Error.Message);
            OnSortingComplete(e.Cancelled);
        }
        // візуалізація обміну двох цілих значень
        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // номери елементів запаковані в property ProgressPercentage
            int i = e.ProgressPercentage / 1000;
            int j = e.ProgressPercentage % 1000;
            OnSortingExchange(arrayToSort[i], arrayToSort[j], i, j);
        }
    }

    // типи для аргументів власних подій
    public class SortingExchangeEventArgs : EventArgs
    {
        public int FirstValue { get; set; }
        public int SecondValue { get; set; }
        public int FirstIndex { get; set; }
        public int SecondIndex { get; set; }
        public SortingExchangeEventArgs(int firstX, int secondX, int i, int j)
        {
            FirstValue = firstX;
            SecondValue = secondX;
            FirstIndex = i;
            SecondIndex = j;
        }
    }
    public class SortingCompleteEventArgs : EventArgs
    {
        public bool Canceled { get; set; }
        public SortingCompleteEventArgs(bool state)
        {
            Canceled = state;
        }
    }
}
