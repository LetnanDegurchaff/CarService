using System;
using System.Collections.Generic;

namespace CarService
{
    class UserUtils
    {
        static private Random _random = new Random();

        static public int GenerateRandomNumber(int min, int max) =>
            _random.Next(min, max);

        public static void WriteColoredText(string text, ConsoleColor color)
        {
            ConsoleColor tempColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = tempColor;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            CarService carService = new CarService();
            carService.Work();
        }
    }

    class CarService
    {
        private const string AcceptCommand = "1";
        private const string RefuseCommand = "2";

        private Dictionary<string, Container> _detailsContainers = new Dictionary<string, Container>();
        private DetailCreator _detailCreator = new DetailCreator();
        private Queue<Car> _cars = new Queue<Car>();

        private int _money = 300;
        private int _penalty = 50;
        private float _workCostCoefficient = 1.2f;
        private int _defaultContainersDetailCount = 10;

        public CarService()
        {
            IReadOnlyDetail[] detailsList = _detailCreator.CreateDetails();

            foreach (IReadOnlyDetail detail in detailsList)
            {
                _detailsContainers.Add(
                    detail.Name,
                    new Container(detail.CreateClone(), _defaultContainersDetailCount));
            }
        }

        private bool IsNegativeBalance => _money <= 0;

        public void Work()
        {
            bool IsWantContinue = true;
            bool isInputCorrect;

            do
            {
                BuyDetails();
                InviteClients();

                while (_cars.Count > 0)
                {
                    Console.Clear();

                    ShowBaseInfo();
                    ShowStorage();
                    ShowClientCarInfo();

                    Car car = _cars.Dequeue();

                    do
                    {
                        isInputCorrect = true;

                        Console.WriteLine($"Вы желаете взяться за ремонт? " +
                            $"(Да - {AcceptCommand} | Нет - {RefuseCommand})");
                        string input = Console.ReadLine();

                        switch (input)
                        {
                            case AcceptCommand:
                                RepairCar(car);
                                break;

                            case RefuseCommand:
                                RefuseRepair();
                                break;

                            default:
                                isInputCorrect = false;
                                break;
                        }

                        UserUtils.WriteColoredText("Команда не верна", ConsoleColor.Red);
                    }
                    while (isInputCorrect == false);
                }

                Console.Clear();

                if (IsNegativeBalance == false)
                {
                    do
                    {
                        isInputCorrect = true;

                        Console.Clear();

                        Console.WriteLine("Рабочий день окончен, продолжить работу? " +
                        $"(Да - {AcceptCommand} | Нет - {RefuseCommand})");

                        string input = Console.ReadLine();

                        switch (input)
                        {
                            case AcceptCommand:
                                continue;

                            case RefuseCommand:
                                IsWantContinue = false;
                                break;

                            default:
                                isInputCorrect = false;
                                break;
                        }

                        UserUtils.WriteColoredText("Команда не верна", ConsoleColor.Red);
                    }
                    while (isInputCorrect == false);
                }
                else
                {
                    Console.WriteLine("Вы проиграли");
                    Console.ReadLine();
                }
            }
            while (IsWantContinue && (IsNegativeBalance == false));
        }

        private void BuyDetails()
        {
            bool isInputCorrect = true;
            List<string> detailsNames = new List<string>();

            foreach (string detailName in _detailsContainers.Keys)
            {
                detailsNames.Add(detailName);
            }

            do
            {
                Console.Clear();

                ShowStorage();
                UserUtils.WriteColoredText($"\nБалланс: {_money}", ConsoleColor.Cyan);
                UserUtils.WriteColoredText("\nМеню покупки деталей:", ConsoleColor.Yellow);

                for (int i = 0; i < detailsNames.Count; i++)
                {
                    int detailNameIndex = i + 1;
                    Console.WriteLine($"{detailNameIndex}) {detailsNames[i]}\t" +
                        $"{_detailsContainers[detailsNames[i]].DetailCost}");
                }

                Console.WriteLine($"Введите номер детали чтобы купить одну или введите не число, чтобы продолжить");

                int inputNumber;

                if (int.TryParse(Console.ReadLine(), out inputNumber))
                {
                    if (inputNumber > 0 && inputNumber <= detailsNames.Count)
                    {
                        inputNumber--;

                        if (_detailsContainers[detailsNames[inputNumber]].IsFull == false)
                        {
                            _detailsContainers[detailsNames[inputNumber]].IncreaseCount();
                            _money -= _detailsContainers[detailsNames[inputNumber]].DetailCost;
                        }
                    }
                }
                else
                {
                    isInputCorrect = false;
                }
            }
            while (isInputCorrect);
        }

        private void ShowStorage()
        {
            int detailIndex = 1;

            UserUtils.WriteColoredText("Содержимое хранилища:", ConsoleColor.Green);

            foreach (KeyValuePair<string, Container> detailSection in _detailsContainers)
            {
                Console.WriteLine($" {detailIndex})\t" +
                    $"{detailSection.Key}  \t\t" +
                    $"{detailSection.Value.Count} шт.");
                detailIndex++;
            }
        }

        private void ShowClientCarInfo()
        {
            Car clientCar = _cars.Peek();
            IReadOnlyDetail brokenDetail = clientCar.GetBrokenDetail();

            Console.Write("\nУ клиента сломано: ");
            UserUtils.WriteColoredText(brokenDetail.Name, ConsoleColor.Red);

            float workCost = brokenDetail.Cost * _workCostCoefficient;
            UserUtils.WriteColoredText($"Стоимость ремонта: {workCost}", ConsoleColor.Green);
        }

        private void ShowBaseInfo()
        {
            UserUtils.WriteColoredText("\t\tДобро пожаловать в автомастерскую", ConsoleColor.Green);

            Console.WriteLine($"Клиентов в очереди: {_cars.Count}");
            UserUtils.WriteColoredText($"Балланс: {_money}\n", ConsoleColor.Cyan);
        }

        private void RepairCar(Car car)
        {
            bool isInputCorrect;

            Console.Clear();

            ShowStorage();
            Console.WriteLine("\nКакую деталь со склада взять для починки (Введите номер)");

            do
            {
                int selectedDetailNumber = GetNumber();
                isInputCorrect = true;

                if (selectedDetailNumber > 0 && selectedDetailNumber <= _detailsContainers.Count)
                {
                    ReplaceDetail(selectedDetailNumber, car);
                }
                else
                {
                    Console.WriteLine("Нет такой детали");
                    isInputCorrect = false;
                }
            }
            while (isInputCorrect == false);
        }

        private void ReplaceDetail(int selectedDetailNumber, Car car)
        {
            IReadOnlyDetail brokenDetail = car.GetBrokenDetail();

            int selectedDetailIndex = --selectedDetailNumber;
            float workCost = brokenDetail.Cost * _workCostCoefficient;

            List<string> detailsTypes = new List<string>();

            foreach (string detailSectionType in _detailsContainers.Keys)
            {
                detailsTypes.Add(detailSectionType);
            }

            if (brokenDetail.Name == detailsTypes[selectedDetailIndex])
            {
                IReadOnlyDetail repairedDetail = car.GetSelectedDetail(selectedDetailIndex);

                if (TryGetNewDetail(_detailsContainers[detailsTypes[selectedDetailIndex]], ref repairedDetail))
                {
                    TakeMoney(Convert.ToInt32(workCost));
                    Console.WriteLine("Вы поменяли неисправную деталь, " +
                        $"вы получаете {workCost} единиц условной валюты");
                }
                else
                {
                    Console.WriteLine("У вас на складе нет таких деталей, " +
                        $"вы получаете {workCost} единиц условной валюты");
                }
            }
            else
            {
                PayPenalty();
                Console.WriteLine("Вы пытались поменять исправную деталь, " +
                    $"вы выплачиваете штраф {_penalty} единиц условной валюты");
            }

            Console.ReadLine();
        }

        private void RefuseRepair()
        {
            PayPenalty();
            Console.WriteLine("Вы отказались обслуживать клиента, " +
                 $"вы выплачиваете штраф {_penalty} единиц условной валюты");

            Console.ReadLine();
        }

        private bool TryGetNewDetail(Container detailsContainer, ref IReadOnlyDetail repairedDetail)
        {
            if (detailsContainer.Count > 0)
            {
                detailsContainer.DecreaseCount();
                repairedDetail = _detailCreator.CreateDetail(repairedDetail.Name);
                return true;
            }

            return false;
        }

        private void InviteClients()
        {
            int minClientsCount = 5;
            int maxClientsCount = 10;

            int clientsCount = UserUtils.GenerateRandomNumber
                (minClientsCount, maxClientsCount - 1);

            for (int i = 0; i < clientsCount; i++)
            {
                _cars.Enqueue(new Car());
            }
        }

        private int GetNumber()
        {
            int number;

            while (int.TryParse(Console.ReadLine(), out number) == false)
                Console.WriteLine("Ошибка ввода!");

            return number;
        }

        private void PayPenalty()
        {
            _money -= _penalty;
        }

        private void TakeMoney(int money)
        {
            _money += money;
        }
    }

    class Car
    {
        private IReadOnlyDetail[] _details;

        public Car()
        {
            DetailCreator detailCreator = new DetailCreator();

            _details = detailCreator.CreateBrokenCarDetails();
        }

        public IReadOnlyDetail GetBrokenDetail()
        {
            foreach (IReadOnlyDetail detail in _details)
            {
                if (detail.IsWorkable == false)
                    return detail;
            }

            return null;
        }

        public IReadOnlyDetail GetSelectedDetail(int selectedDetailIndex)
        {
            return _details[selectedDetailIndex];
        }
    }

    class Container
    {
        public const int MaxDetailsCount = 15;

        private IReadOnlyDetail _detail;

        public Container(IReadOnlyDetail detail, int count)
        {
            _detail = detail;
            Count = count;
        }

        public int Count { get; private set; }
        public int DetailCost => _detail.Cost;
        public bool IsFull => Count == MaxDetailsCount;

        public void IncreaseCount()
        {
            if (Count < MaxDetailsCount)
                Count++;
        }

        public void DecreaseCount()
        {
            if (Count > 0)
                Count--;
        }
    }

    interface IReadOnlyDetail
    {
        string Name { get; }
        int Cost { get; }
        bool IsWorkable { get; }

        Detail CreateClone();
    }

    class Detail : IReadOnlyDetail
    {
        public Detail(string name, int cost)
        {
            Name = name;
            Cost = cost;
            IsWorkable = true;
        }

        private Detail(string name, int cost, bool isWorkable)
        {
            Name = name;
            Cost = cost;
            IsWorkable = isWorkable;
        }

        public string Name { get; private set; }
        public int Cost { get; private set; }
        public bool IsWorkable { get; private set; }

        public Detail CreateClone() => new Detail(Name, Cost, IsWorkable);

        public void Broke() => IsWorkable = false;
    }

    class DetailCreator
    {
        private Detail[] _details =
        {
            new Detail("Двигатель", 200),
            new Detail("Трансмиссия", 120),
            new Detail("Подвеска", 70)
        };

        public IReadOnlyDetail[] CreateDetails()
        {
            IReadOnlyDetail[] details = new IReadOnlyDetail[_details.Length];

            for (int i = 0; i < _details.Length; i++)
                details[i] = _details[i].CreateClone();

            return details;
        }

        public IReadOnlyDetail[] CreateBrokenCarDetails()
        {
            Detail[] details = new Detail[_details.Length];
            int brokenDetail = UserUtils.GenerateRandomNumber(0, _details.Length);

            for (int i = 0; i < _details.Length; i++)
            {
                details[i] = _details[i].CreateClone();

                if (i == brokenDetail)
                    details[i].Broke();
            }

            return details;
        }

        public IReadOnlyDetail CreateDetail(string name)
        {
            foreach (IReadOnlyDetail detail in _details)
                if (detail.Name == name)
                    return detail.CreateClone();

            return null;
        }
    }
}