using AutoMapper;
using CarDealer.Data;
using CarDealer.Dtos.Export;
using CarDealer.Dtos.Import;
using CarDealer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<CarDealerProfile>();
            });
            var carsXml = File.ReadAllText("../../../Datasets/cars.xml");
            var customersXml = File.ReadAllText("../../../Datasets/customers.xml");
            var partsXml = File.ReadAllText("../../../Datasets/parts.xml");
            var salesXml = File.ReadAllText("../../../Datasets/sales.xml");
            var suppliersXml = File.ReadAllText("../../../Datasets/suppliers.xml");

            using (CarDealerContext context = new CarDealerContext())
            {
                //context.Database.EnsureDeleted();
                //context.Database.EnsureCreated();
                //Console.WriteLine(ImportSuppliers(context, suppliersXml));
                //Console.WriteLine(ImportParts(context, partsXml));
                //Console.WriteLine(ImportCars(context, carsXml));
                //Console.WriteLine(ImportCustomers(context, customersXml));
                //Console.WriteLine(ImportSales(context, salesXml));
                //Console.WriteLine(GetCarsWithDistance(context));
                //Console.WriteLine(GetCarsFromMakeBmw(context));
                //Console.WriteLine(GetLocalSuppliers(context));
                //Console.WriteLine(GetCarsWithTheirListOfParts(context));
                //Console.WriteLine(GetTotalSalesByCustomer(context));
                Console.WriteLine(GetSalesWithAppliedDiscount(context));

            }
        }

        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ImportSuppliersDto[]),
                new XmlRootAttribute("Suppliers"));

            var suppliersDeserialized = (ImportSuppliersDto[])serializer
                .Deserialize(new StringReader(inputXml));

            var suppliers = new List<Supplier>();

            foreach (var supplier in suppliersDeserialized)
            {
                var currentSupplier = Mapper.Map<Supplier>(supplier);
                suppliers.Add(currentSupplier);
            }

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Count}";
        }

        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ImportPartsDto[]),
                new XmlRootAttribute("Parts"));

            var partsDeserialized = (ImportPartsDto[])serializer
                .Deserialize(new StringReader(inputXml));

            var parts = new List<Part>();

            foreach (var part in partsDeserialized.Where(e => e.SupplierId <= context.Suppliers.Count()))
            {
                var currentPart = Mapper.Map<Part>(part);
                parts.Add(currentPart);
            }

            context.Parts.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Count}";
        }

        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ImportCarsDto[]),
                new XmlRootAttribute("Cars"));

            var carsDeserialized = (ImportCarsDto[])serializer
                .Deserialize(new StringReader(inputXml));

            var cars = new List<Car>();
            var carParts = new List<PartCar>();

            foreach (var car in carsDeserialized)
            {
                var currentCar = Mapper.Map<Car>(car);
                foreach (var part in car.Parts)
                {
                    if (!currentCar.PartCars.Select(e => e.PartId).Contains(part.PartId)
                        && part.PartId <= context.Parts.Count())
                    {
                        currentCar.PartCars.Add(new PartCar()
                        {
                            PartId = part.PartId
                        });
                    }
                }
                cars.Add(currentCar);
            }

            context.AddRange(cars);
            context.SaveChanges();

            return $"Successfully imported {cars.Count}";
        }

        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ImportCustomersDto[]),
                new XmlRootAttribute("Customers"));

            var customersDeserialized = (ImportCustomersDto[])serializer
                .Deserialize(new StringReader(inputXml));

            var customers = new List<Customer>();

            foreach (var customer in customersDeserialized)
            {
                var currentCustomer = Mapper.Map<Customer>(customer);
                customers.Add(currentCustomer);
            }

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count}";
        }

        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ImportSalesDto[]),
                new XmlRootAttribute("Sales"));

            var salesDeserialized = (ImportSalesDto[])serializer
                .Deserialize(new StringReader(inputXml));

            var sales = new List<Sale>();

            foreach (var sale in salesDeserialized.Where(e => context.Cars.Any(n => n.Id == e.CarId) && e.Discount <= 100))
            {
                var currentSale = Mapper.Map<Sale>(sale);
                sales.Add(currentSale);
            }

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Count}";
        }

        public static string GetCarsWithDistance(CarDealerContext context)
        {
            var cars = context.Cars
                .Where(e => e.TravelledDistance > 2000000)
                .OrderBy(e => e.Make)
                .ThenBy(e => e.Model)
                .Select(e => new ExportCarsWithDistanceDto
                {
                    Make = e.Make,
                    Model = e.Model,
                    TravelledDistance = e.TravelledDistance
                })
                .Take(10)
                .ToArray();

            XmlSerializer serializer = new XmlSerializer(typeof(ExportCarsWithDistanceDto[]),
                new XmlRootAttribute("cars"));

            StringBuilder sb = new StringBuilder();

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            serializer.Serialize(new StringWriter(sb), cars, ns);

            return sb.ToString();
        }

        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            //Get all cars from make BMW and order them by model alphabetically and by travelled distance descending.
            //Return the list of suppliers to XML in the format provided below.

            var cars = context.Cars
                .Where(e => e.Make == "BMW")
                .OrderBy(e => e.Model)
                .ThenByDescending(e => e.TravelledDistance)
                .Select(e => new ExportCarsFromMakeBMWDto
                {
                    Id = e.Id,
                    Model = e.Model,
                    TravelledDistance = e.TravelledDistance
                })
                .ToArray();

            XmlSerializer serializer = new XmlSerializer(typeof(ExportCarsFromMakeBMWDto[]),
                new XmlRootAttribute("cars"));

            StringBuilder sb = new StringBuilder();

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            serializer.Serialize(new StringWriter(sb), cars, ns);

            return sb.ToString();
        }

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            //Get all suppliers that do not import parts from abroad.Get their id, name and the number of parts they can offer to supply.
            //Return the list of suppliers to XML in the format provided below.

            var suppliers = context.Suppliers
               .Where(e => e.IsImporter == false)
               .Select(e => new ExportLocalSuppliersDto
               {
                   Id = e.Id,
                   Name = e.Name,
                   PartsCount = e.Parts.Count()
               })
               .ToArray();

            XmlSerializer serializer = new XmlSerializer(typeof(ExportLocalSuppliersDto[]),
                new XmlRootAttribute("suppliers"));

            StringBuilder sb = new StringBuilder();

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            serializer.Serialize(new StringWriter(sb), suppliers, ns);

            return sb.ToString();
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {

            //Get all cars along with their list of parts.For the car get only make, model and 
            //travelled distance and for the parts get only name and price and sort all pars by 
            //price(descending).Sort all cars by travelled distance(descending) then by model(ascending).Select top 5 records.

            var cars = context.Cars
                 .OrderByDescending(e => e.TravelledDistance)
                 .ThenBy(e => e.Model)
                .Select(e => new ExportCarsWithListOfPartsDto
                {
                    Make = e.Make,
                    Model = e.Model,
                    TravelledDistance = e.TravelledDistance,
 
                    Parts =

                        e.PartCars.Select(p => new ExportCarsPartsListDto
                        {
                            Name = p.Part.Name,
                            Price = p.Part.Price
                        })
                        .OrderByDescending(p => p.Price)
                        .ToArray()
                })
                .Take(5)
                .ToArray();
               

            XmlSerializer serializer = new XmlSerializer(typeof(ExportCarsWithListOfPartsDto[]),
                new XmlRootAttribute("cars"));

            StringBuilder sb = new StringBuilder();

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            serializer.Serialize(new StringWriter(sb), cars, ns);

            return sb.ToString();
        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            //Get all customers that have bought at least 1 car and get their names, bought cars count and 
            //total spent money on cars.Order the result list by total spent money descending.

           var customers = context.Customers
                .Where(e => e.Sales.Any())
                 .OrderByDescending(e => e.Sales.Sum(s => s.Car.PartCars.Sum(n => n.Part.Price)))
                .Select(e => new ExportTotalSalesByCustomerDto
                {
                    Name = e.Name,
                    BoughtCars = e.Sales.Count(),
                    //SpentMoney = e.Sales.Select(s => s.Car.PartCars.Sum(n => n.Part.Price)),
                    SpentMoney = e.Sales.Sum(s => s.Car.PartCars.Sum(n => n.Part.Price)),
                    
                })
                .ToArray();


            XmlSerializer serializer = new XmlSerializer(typeof(ExportTotalSalesByCustomerDto[]),
                new XmlRootAttribute("customers"));

            StringBuilder sb = new StringBuilder();

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            serializer.Serialize(new StringWriter(sb), customers, ns);

            return sb.ToString();
        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            //Get all sales with information about the car, customer and price of the sale with and without discount.

            var sales = context.Sales
                .Select(e => new ExportSalesWithAppliedDiscountDto
                {
                    Car = new ExportCarsAttributeDto
                    {
                        Make = e.Car.Make,
                        Model = e.Car.Model,
                        TravelledDistance = e.Car.TravelledDistance
                    },
                    Discount = e.Discount,
                    CustomerName = e.Customer.Name,
                    Price = e.Car.PartCars.Sum(p => p.Part.Price),
                    PriceWithDiscount = ((e.Car.PartCars.Sum(p => p.Part.Price) * (1 - e.Discount/100))).ToString().TrimEnd('0'),
                })
                .ToArray();

            //var sales2 = new TestDto
            //{
            //    sales = sales,
            //   ExportSaleDiscount = string.Empty
            //};

            XmlSerializer serializer = new XmlSerializer(typeof(ExportSalesWithAppliedDiscountDto[]),
                new XmlRootAttribute("sales"));

            StringBuilder sb = new StringBuilder();

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            serializer.Serialize(new StringWriter(sb), sales, ns);

            return sb.ToString();
        }
    }
}