using System;
using System.Collections.Generic;

namespace CarService
{
    static class UserUtils
    {
        private static Random _random = new Random();

        public static int GenerateRandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

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
        private const float WorkCostCoefficient = 1.2f;
        private const int Penalty = 20;

        private const string AcceptCommand = "1";
        private const string RefuseCommand = "2";

        private int _money;
        private Dictionary<Type, Queue<Detail>> _detailsSections;
        private Dictionary<Type, Func<Detail>> _detailsCreators;
        private Queue<Car> _clientsCars;

        private DetailCreater _detailCreator;
        private CarCreater _carCreater;

        public CarService()
        {
            Initialize();
            FillStorage();
        }

        public void Work()
        {
            LetClientsIn();
            ShowStorage();

            while (_clientsCars.Count > 0)
            {
                bool isInputCorrect;

                Console.Clear();

                ShowBaseInfo();
                ShowStorage();
                ShowClientCarInfo();

                Car clientCar = _clientsCars.Dequeue();

                do
                {
                    isInputCorrect = true;
                    string input = Console.ReadLine();

                    switch (input)
                    {
                        case AcceptCommand:
                            RepairCar(clientCar);
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
        }

        private void FillStorage()
        {
            int minCertainDetailCount = 1;
            int maxCertainDetailCount = 5;
            int certainDetailCount;

            foreach (KeyValuePair<Type, Queue<Detail>> detailsSection in _detailsSections)
            {
                certainDetailCount = UserUtils.GenerateRandomNumber(minCertainDetailCount, maxCertainDetailCount + 1);

                for (int i = 0; i < certainDetailCount; i++)
                {
                    detailsSection.Value.Enqueue(_detailsCreators[detailsSection.Key]());
                }
            }
        }

        private void Initialize()
        {
            _detailsSections = new Dictionary<Type, Queue<Detail>>();
            _clientsCars = new Queue<Car>();
            _detailCreator = new DetailCreater();
            _carCreater = new CarCreater();

            _money = 500;

            _detailsCreators = new Dictionary<Type, Func<Detail>>();

            _detailsCreators.Add(typeof(Engine), _detailCreator.Create<Engine>);
            _detailsCreators.Add(typeof(GearBox), _detailCreator.Create<GearBox>);
            _detailsCreators.Add(typeof(Suspension), _detailCreator.Create<Suspension>);

            foreach (Type detailType in _detailsCreators.Keys)
            {
                _detailsSections.Add(detailType,
                    new Queue<Detail>());
            }
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

                if (selectedDetailNumber > 0 && selectedDetailNumber <= _detailsSections.Count)
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

        private void RefuseRepair()
        {
            PayPenalty();
            Console.WriteLine("Вы отказались обслуживать клиента, " +
                 $"вы выплачиваете штраф {Penalty} единиц условной валюты");

            Console.ReadLine();
        }

        private void ReplaceDetail(int selectedDetailNumber, Car car)
        {
            int selectedDetailIndex = --selectedDetailNumber;
            float workCost = car.GetBrokenDetailCost() * WorkCostCoefficient;

            List<Type> detailsTypes = new List<Type>();

            foreach (Type detailSectionType in _detailsSections.Keys)
            {
                detailsTypes.Add(detailSectionType);
            }

            if (car.GetBrokenDetailName() == detailsTypes[selectedDetailIndex].Name)
            {
                Detail repairedDetail = car.GetSelectedDetail(selectedDetailIndex);
                repairedDetail = _detailsSections[detailsTypes[selectedDetailIndex]].Dequeue();
                TakeMoney(Convert.ToInt32(workCost));
                Console.WriteLine("Вы поменяли неисправную деталь, " +
                    $"вы получаете {workCost} единиц условной валюты");
            }
            else
            {
                PayPenalty();
                Console.WriteLine("Вы пытались поменять исправную деталь, " +
                    $"вы выплачиваете штраф {Penalty} единиц условной валюты");
            }

            Console.ReadLine();
        }

        private void ShowBaseInfo()
        {
            UserUtils.WriteColoredText("\t\tДобро пожаловать в автомастерскую", ConsoleColor.Green);

            Console.WriteLine($"Клиентов в очереди: {_clientsCars.Count}");
            UserUtils.WriteColoredText($"Балланс: {_money}\n", ConsoleColor.Cyan);
        }

        private void ShowStorage()
        {
            int detailIndex = 1;

            UserUtils.WriteColoredText("Содержимое хранилища:", ConsoleColor.Green);

            foreach (KeyValuePair<Type, Queue<Detail>> detailSection in _detailsSections)
            {
                Console.WriteLine($" {detailIndex})\t{detailSection.Key.Name}  \t\t{detailSection.Value.Count} шт.");
                detailIndex++;
            }
        }

        private void ShowClientCarInfo()
        {
            Car clientCar = _clientsCars.Peek();
            Console.Write("\nУ клиента сломано: ");
            UserUtils.WriteColoredText(clientCar.GetBrokenDetailName(), ConsoleColor.Red);

            float workCost = clientCar.GetBrokenDetailCost() * WorkCostCoefficient;
            UserUtils.WriteColoredText($"Стоимость ремонта: {workCost}", ConsoleColor.Green);
            Console.WriteLine($"Вы желаете взяться за ремонт? (Да - {AcceptCommand} | Нет - {RefuseCommand})");
        }

        private void LetClientsIn()
        {
            int minCustomers = 5;
            int maxCustomers = 10;
            int IncomingCustomers = UserUtils.GenerateRandomNumber(minCustomers, maxCustomers + 1);

            for (int i = 0; i < IncomingCustomers; i++)
            {
                _clientsCars.Enqueue(_carCreater.CreateBrokenCar());
            }
        }

        private int GetNumber()
        {
            int number;

            while (int.TryParse(Console.ReadLine(), out number) == false)
                Console.WriteLine("Ошибка ввода!");

            return number;
        }

        private void TakeMoney(int money)
        {
            _money += money;
        }

        private void PayPenalty()
        {
            _money -= Penalty;
        }
    }

    class Car
    {
        private List<Detail> _details;

        public Car(Detail engine, Detail gearBox, Detail suspension)
        {
            _details = new List<Detail>
            {
                engine, gearBox, suspension
            };
        }

        public Detail GetSelectedDetail(int selectedDetailIndex)
        {
            return _details[selectedDetailIndex];
        }

        public int GetBrokenDetailCost()
        {
            foreach (Detail detail in _details)
            {
                if (detail.IsWorkable == false)
                    return detail.Cost;
            }

            return 0;
        }

        public string GetBrokenDetailName()
        {
            foreach (Detail detail in _details)
            {
                if (detail.IsWorkable == false)
                    return detail.GetType().Name;
            }

            return null;
        }

        public void BrokeRandomDetail()
        {
            int brokedDetailNumber = UserUtils.GenerateRandomNumber(0, _details.Count);

            _details[brokedDetailNumber].Broke();
        }
    }

    class CarCreater
    {
        private DetailCreater _detailCreater;

        public CarCreater()
        {
            _detailCreater = new DetailCreater();
        }

        public Car CreateBrokenCar()
        {
            Car car = new Car(
                _detailCreater.Create<Engine>(),
                _detailCreater.Create<GearBox>(),
                _detailCreater.Create<Suspension>());

            car.BrokeRandomDetail();

            return car;
        }
    }

    abstract class Detail
    {
        public Detail(int cost)
        {
            Cost = cost;
            IsWorkable = true;
        }
        public int Cost { get; private set; }
        public bool IsWorkable { get; private set; }

        public void Broke()
        {
            IsWorkable = false;
        }
    }

    class Engine : Detail
    {
        public Engine() : base(150) { }
    }

    class GearBox : Detail
    {
        public GearBox() : base(150) { }
    }

    class Suspension : Detail
    {
        public Suspension() : base(150) { }
    }

    class DetailCreater
    {
        public T Create<T>() where T : Detail, new()
        {
            return new T();
        }
    }
}