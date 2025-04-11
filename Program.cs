﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class StudentEnumerator : IEnumerator<string>
{
  private readonly ArrayList _subjects = new();
  private int _position = -1;

  public StudentEnumerator(ArrayList tests, ArrayList exams)
  {
    foreach (Exam exam in exams)
      if (!_subjects.Contains(exam.Subject))
        _subjects.Add(exam.Subject);

    foreach (Test test in tests)
      if (!_subjects.Contains(test.Subject))
        _subjects.Add(test.Subject);
  }

  public string Current
  {
    get
    {
      if (_position < 0 || _position >= _subjects.Count)
        throw new InvalidOperationException();
      return (string)_subjects[_position]!;
    }
  }

  object IEnumerator.Current => Current;

  public bool MoveNext()
  {
    _position++;
    return _position < _subjects.Count;
  }

  public void Reset()
  {
    _position = -1;
  }

  public void Dispose()
  {
    // Нічого не звільняється — але метод потрібен
  }
}

public interface IDateAndCopy
{
  DateTime Date { get; init; }
  object DeepCopy();
}

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

  public object DeepCopy()
  {
    return new Exam(Subject, Grade, ExamDate);
  }
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

  public virtual string ToShortString() => $"{Name} {Surname}";

  public override bool Equals(object? obj)
  {
    if (obj is not Person other)
      return false;

    return Name == other.Name &&
           Surname == other.Surname &&
           BirthDate == other.BirthDate;
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(Name, Surname, BirthDate);
  }

  public static bool operator ==(Person? left, Person? right)
  {
    if (ReferenceEquals(left, right))
      return true;

    if (left is null || right is null)
      return false;

    return left.Equals(right);
  }

  public static bool operator !=(Person? left, Person? right)
  {
    return !(left == right);
  }

  public virtual object DeepCopy()
  {
    return new Person(Name, Surname, BirthDate);
  }
}

public class Test
{
  public string Subject { get; set; }
  public bool Passed { get; set; }

  public Test(string subject, bool passed)
  {
    Subject = subject;
    Passed = passed;
  }

  public Test() : this("Unknown", false) { }

  public override string ToString()
  {
    return $"Subject: {Subject}, Passed: {(Passed ? "Yes" : "No")}";
  }
}

public class Student : Person, IDateAndCopy, IEnumerable<string>
{
  private Education _education;
  private int _group;
  private ArrayList _tests = new();
  private ArrayList _exams = new();

  public DateTime Date { get; init; }

  // Конструктор з параметрами типу Person + інші
  public Student(Person person, Education education, int group)
    : base(person.Name, person.Surname, person.BirthDate)
  {
    _education = education;
    _group = group;
  }

  // Конструктор за замовчуванням
  public Student() : this(new Person(), Education.Bachelor, 0) { }



  public IEnumerable GetAllResults()
  {
    foreach (var test in _tests)
      yield return test;
    foreach (var exam in _exams)
      yield return exam;
  }

  public IEnumerable<Exam> GetExamsAbove(int minGrade)
  {
    foreach (Exam exam in _exams)
    {
      if (exam.Grade > minGrade)
        yield return exam;
    }
  }


  public IEnumerator<string> GetEnumerator()
  {
    return new StudentEnumerator(_tests, _exams);
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }



  public IEnumerable PassedItems()
  {
    foreach (Exam exam in _exams)
      if (exam.Grade > 2)
        yield return exam;

    foreach (Test test in _tests)
      if (test.Passed)
        yield return test;
  }

  public IEnumerable<Test> PassedTestsWithExam()
  {
    foreach (Test test in _tests)
    {
      bool hasExam = _exams.Cast<Exam>().Any(e => e.Subject == test.Subject && e.Grade > 2);
      if (hasExam)
        yield return test;
    }
  }





  // Властивість Person
  public Person PersonalInfo => this;

  // Властивість Education
  public Education Education => _education;

  // Властивість Group
  public int Group
  {
    get => _group;
    init
    {
      if (value < 100 || value > 699)
        throw new ArgumentOutOfRangeException(nameof(Group), $"Група має бути в межах від 100 до 699. Отримано: {value}");
      _group = value;
    }
  }

  // Властивість ExamList
  public ArrayList Exams => _exams;

  // Властивість TestList
  public ArrayList Tests => _tests;

  // Властивість AverageGrade (тільки get)
  public double AverageGrade
  {
    get
    {
      if (_exams.Count == 0) return 0;
      double sum = 0;
      foreach (Exam exam in _exams)
        sum += exam.Grade;
      return sum / _exams.Count;
    }
  }

  // Метод додавання екзаменів
  public void AddExams(params Exam[] exams)
  {
    if (exams is null || exams.Length == 0)
      return;
    Exams.AddRange(exams);
  }

  public void AddTests(params Test[] tests)
  {
    if (tests is null || tests.Length == 0)
      return;
    Tests.AddRange(tests);
  }

  // ToString (перевизначений)
  public override string ToString()
  {
    string examsInfo = Exams.Count > 0 ? string.Join("; ", Exams.Cast<Exam>().Select(e => e.ToString())) : "No exams";
    string testsInfo = Tests.Count > 0 ? string.Join("; ", Tests.Cast<Test>().Select(t => t.ToString())) : "No tests";
    return $"{base.ToString()}, Education: {_education}, Group: {_group}, Exams: [{examsInfo}], Tests: [{testsInfo}]";
  }

  // ToShortString (перевизначений)
  public override string ToShortString()
  {
    return $"{base.ToString()}, Education: {Education}, Group: {Group}, Avg. Grade: {AverageGrade:F2}";
  }

  // DeepCopy
  public override object DeepCopy()
  {
    Student copy = new(PersonalInfo, Education, Group);
    foreach (Exam exam in Exams)
      copy.Exams.Add((Exam)exam.DeepCopy());
    foreach (Test test in Tests)
      copy.Tests.Add(new Test(test.Subject, test.Passed));
    return copy;
  }

  // Індексатор
  public bool this[Education edu] => Education == edu;
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

    var person = new Person("Iван", "Петров", new DateTime(2002, 5, 12));
    var student = new Student(person, Education.Bachelor, 101);

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

    student.AddTests(
    new Test("Math", true),
    new Test("OOP", false)
);

    Console.WriteLine("\nПiсля додавання екзаменiв:");
    Console.WriteLine(student.ToString());

    Console.WriteLine("\nЛабораторна робота №2:\n");

    var p1 = new Person("Олег", "Ковальчук", new DateTime(2000, 1, 1));
    var p2 = new Person("Олег", "Ковальчук", new DateTime(2000, 1, 1));
    var p3 = new Person("Інна", "Ковальчук", new DateTime(2000, 1, 1));




    Console.WriteLine($"p1 == p2: {p1 == p2}"); // true
    Console.WriteLine($"p1 != p3: {p1 != p3}"); // true
    Console.WriteLine($"p1.Equals(p2): {p1.Equals(p2)}"); // true
    Console.WriteLine($"HashCode p1: {p1.GetHashCode()} | p2: {p2.GetHashCode()}"); // однакові

    Console.WriteLine("\nВластивості типу Person для об'єкта Student:");
    Console.WriteLine(student.PersonalInfo);

    Console.WriteLine("\nКопiя студента (до змiни оригіналу):");
    var studentCopy = (Student)student.DeepCopy();
    Console.WriteLine(studentCopy);

    // Змінюємо оригінал
    student.Exams[0] = new Exam("Changed", 20, DateTime.Today);

    Console.WriteLine("\nОригiнальний студент (пiсля змiни):");
    Console.WriteLine(student);
    Console.WriteLine("\nКопiя студента (має залишитись без змiн):");
    Console.WriteLine(studentCopy);

    try
    {
      var brokenStudent = new Student(new Person(), Education.Master, 50); // некоректне значення
    }
    catch (ArgumentOutOfRangeException ex)
    {
      Console.WriteLine($"\nПомилка: {ex.Message}");
    }

    Console.WriteLine("\nЕкзамени з оцiнкою > 3:");
    foreach (Exam exam in student.GetExamsAbove(3))
    {
      Console.WriteLine(exam);
    }
    Console.WriteLine("\nУнікальні предмети (StudentEnumerator):");
    foreach (string subject in student)
    {
      Console.WriteLine(subject);
    }

    foreach (object item in student.PassedItems())
    {
      Console.WriteLine(item);
    }

    foreach (Test test in student.PassedTestsWithExam())
    {
      Console.WriteLine(test);
    }




  }
}
