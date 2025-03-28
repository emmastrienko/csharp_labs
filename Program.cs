﻿using System;
using System.Linq;

public enum Education
{
  Master,
  Bachelor,
  SecondEducation
}

public class Exam
{
  public string Subject { get; init; }
  public int Grade { get; set; }
  public DateTime ExamDate { get; init; }

  public Exam(string subject, int grade, DateTime examDate)
  {
    Subject = subject;
    Grade = grade;
    ExamDate = examDate;
  }

  public Exam() : this("Unknown", 0, DateTime.MinValue) { }

  public override string ToString() => $"Subject: {Subject}, Grade: {Grade}, Date: {ExamDate:yyyy-MM-dd}";
}

public class Person
{
  public string Name { get; init; }
  public string Surname { get; init; }
  public DateTime BirthDate { get; init; }

  public Person(string name, string surname, DateTime birthDate)
  {
    Name = name;
    Surname = surname;
    BirthDate = birthDate;
  }

  public Person() : this("Unknown", "Unknown", DateTime.Now) { }

  public override string ToString() => $"{Name} {Surname} ({BirthDate:yyyy-MM-dd})";

  public string ToShortString() => $"{Name} {Surname}";
}

public class Student
{
  private Person _person;
  private Education _education;
  private int _group;
  private Exam[] _exams;

  public Student(Person person, Education education, int group)
  {
    Person = person;
    Education = education;
    Group = group;
    Exams = new Exam[0];
  }

  public Student() : this(new Person(), Education.Bachelor, 0) { }

  // Властивості
  public Person Person { get => _person; init => _person = value; }
  public Education Education { get => _education; init => _education = value; }
  public int Group { get => _group; init => _group = value; }
  public Exam[] Exams { get => _exams; set => _exams = value ?? new Exam[0]; }

  // Властивість тільки для читання – середній бал
  public double AverageGrade => Exams is null || Exams.Length == 0 ? 0 : Exams.Average(e => e.Grade);

  // Індексатор
  public bool this[Education edu] => Education == edu;

  // Додавання іспитів
  public void AddExams(params Exam[] newExams)
  {
    if (newExams is null || newExams.Length == 0)
      return;
    if (Exams is null || Exams.Length == 0)
    {
      Exams = newExams;
      return;
    }
    Exams = Exams.Concat(newExams).ToArray();
  }

  // Віртуальний метод ToString()
  public override string ToString()
  {
    string examsInfo = Exams.Length > 0 ? string.Join("; ", Exams.Select(e => e.ToString())) : "No exams taken";
    return $"{Person}, Education: {Education}, Group: {Group}, Exams: {examsInfo}";
  }

  // Віртуальний метод ToShortString()
  public virtual string ToShortString()
  {
    return $"{Person}, Education: {Education}, Group: {Group}, Avg. Grade: {AverageGrade:F2}";
  }
}

class Program
{
  static void Main()
  {

    Console.WriteLine("Введiть цiле число рядкiв, а потiм стовпцiв, роздiлених пробiлом, комою або крапкою з комою:");

    string? inputLine = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(inputLine))
    {
      Console.WriteLine("Помилка! Введіть два додатних цілих числа.");
      return;
    }

    string[] input = inputLine.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
    if (input.Length < 2 || !int.TryParse(input[0], out int nRows) || !int.TryParse(input[1], out int nColumns) || nRows <= 0 || nColumns <= 0)
    {
      Console.WriteLine("Помилка! Введіть два додатних цілих числа.");
      return;
    }

    var nRowsColumns = nRows * nColumns;
    var oneDimensional = new Exam[nRowsColumns];
    var twoDimensional = new Exam[nRows, nColumns];
    
    var increasingJaggedArray = new Exam[nRows][];

    int acc = 0, rows = 0;
    while (acc < nRowsColumns)
    {
      rows++;
      acc += rows;
    }
    var jaggedArray = new Exam[rows][];

    for (int i = 0; i < rows - 1; i++)
      jaggedArray[i] = new Exam[i + 1];

    jaggedArray[rows - 1] = new Exam[rows - (acc - nRowsColumns)];

    int assignedElements = 0;
    for (int i = 0; i < nRows; i++)
    {
      int remaining = (nRows * nColumns) - assignedElements;
      int rowSize = Math.Min(nColumns, remaining);
      increasingJaggedArray[i] = new Exam[rowSize];
      assignedElements += rowSize;
    }

    for (int i = 0; i < nRowsColumns; i++)
    {
      oneDimensional[i] = new Exam("Math", 0, DateTime.Now);
    }

    for (int i = 0; i < nRows; i++)
    {
      for (int j = 0; j < nColumns; j++)
      {
        twoDimensional[i, j] = new Exam("Math", 0, DateTime.Now);
      }
    }

    for (int i = 0; i < jaggedArray.Length; i++)
    {
      for (int j = 0; j < jaggedArray[i].Length; j++)
      {
        jaggedArray[i][j] = new Exam("Math", 0, DateTime.Now);
      }
    }

    int startTime = Environment.TickCount;
    for (int i = 0; i < nRows * nColumns; i++)
    {
      oneDimensional[i].Grade = 100;
    }
    int oneDimTime = Environment.TickCount - startTime;

    startTime = Environment.TickCount;
    for (int i = 0; i < nRows; i++)
    {
      for (int j = 0; j < nColumns; j++)
      {
        twoDimensional[i, j].Grade = 100;
      }
    }
    int twoDimTime = Environment.TickCount - startTime;

    startTime = Environment.TickCount;
    for (int i = 0; i < jaggedArray.Length; i++)
    {
      for (int j = 0; j < jaggedArray[i].Length; j++)
      {
        jaggedArray[i][j].Grade = 100;
      }
    }
    int jaggedTime = Environment.TickCount - startTime;

    Console.WriteLine("\nЧас виконання:");
    Console.WriteLine($"1-вимiрний: {oneDimTime} мс");
    Console.WriteLine($"2-вимiрний: {twoDimTime} мс");
    Console.WriteLine($"Зубчатий: {jaggedTime} мс");

    var student = new Student(new Person("Iван", "Петров", new DateTime(2002, 5, 12)), Education.Bachelor, 101);
    Console.WriteLine("\nToShortString:");
    Console.WriteLine(student.ToShortString());

    Console.WriteLine("\nIндекс:");
    Console.WriteLine($"Master: {student[Education.Master]}");
    Console.WriteLine($"Bachelor: {student[Education.Bachelor]}");
    Console.WriteLine($"SecondEducation: {student[Education.SecondEducation]}");

    Console.WriteLine("\nToString:");
    Console.WriteLine(student.ToString());

    var exam1 = new Exam("Physics", 85, new DateTime(2024, 6, 1));
    var exam2 = new Exam("Chemistry", 90, new DateTime(2024, 6, 5));
    student.AddExams(exam1, exam2);

    Console.WriteLine("\nПiсля додавання екзаменiв:");
    Console.WriteLine(student.ToString());

  }
}
