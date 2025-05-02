﻿using System;
using System.Linq;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;

public class TestCollections<TKey, TValue>
    where TKey : class
    where TValue : class, IDateAndCopy
{
  public List<TKey> ListKey { get; }
  public List<string> ListString { get; }
  public Dictionary<TKey, TValue> DictKey { get; }
  public Dictionary<string, TValue> DictString { get; }
  public List<TValue> ListValue { get; }  // Add a ListValue property to store the TValue data

  public TestCollections(int size, Func<int, (TKey key, TValue value)> generator)
  {
    ListKey = new List<TKey>(size);
    ListString = new List<string>(size);
    DictKey = new Dictionary<TKey, TValue>(size);
    DictString = new Dictionary<string, TValue>(size);
    ListValue = new List<TValue>(size);  // Initialize the ListValue to store TValue data

    for (int i = 0; i < size; i++)
    {
      var (key, value) = generator(i);
      ListKey.Add(key);
      ListString.Add(key.ToString()!);
      DictKey[key] = value;
      DictString[key.ToString()!] = value;
      ListValue.Add(value);  // Add the TValue (student) to ListValue
    }
  }


  public void TestSearchAll(int v)
  {
    var elementsToSearch = new List<(string name, TKey key)>
    {
        ("Перший", ListKey.First()),
        ("Центральний", ListKey[ListKey.Count / 2]),
        ("Останній", ListKey.Last()),
        ("Відсутній", GenerateMissingKey())
    };

    foreach (var (name, key) in elementsToSearch)
    {
      string keyStr = key.ToString()!;
      Console.WriteLine($"\nПошук елемента: {name}");

      var sw = new System.Diagnostics.Stopwatch();

      sw.Restart();
      ListKey.Contains(key);
      sw.Stop();
      Console.WriteLine($"List<TKey>: {sw.ElapsedTicks} ticks");

      sw.Restart();
      ListString.Contains(keyStr);
      sw.Stop();
      Console.WriteLine($"List<string>: {sw.ElapsedTicks} ticks");

      sw.Restart();
      DictKey.ContainsKey(key);
      sw.Stop();
      Console.WriteLine($"Dictionary<TKey, TValue>: {sw.ElapsedTicks} ticks");

      sw.Restart();
      DictString.ContainsKey(keyStr);
      sw.Stop();
      Console.WriteLine($"Dictionary<string, TValue>: {sw.ElapsedTicks} ticks");

      sw.Restart();
      DictKey.ContainsValue(DictKey.GetValueOrDefault(key)!); // Для коректного елементу
      sw.Stop();
      Console.WriteLine($"Dictionary<TKey, TValue> (ContainsValue): {sw.ElapsedTicks} ticks");
    }
  }

  // Метод генерації ключа, якого точно немає в колекції
  private TKey GenerateMissingKey()
  {
    // В залежності від типу TKey, створити новий унікальний об'єкт
    if (typeof(TKey) == typeof(Person))
    {
      var person = new Person("Інший", "Ключ", new DateTime(1800, 1, 1));
      return (TKey)(object)person;
    }

    throw new InvalidOperationException("Невідомий тип TKey для генерації відсутнього елемента");
  }

}



public class TestCollectionsImmutable<TKey, TValue>
{
  private ImmutableList<TKey> keyList;
  private ImmutableDictionary<TKey, TValue> keyValueDict;

  public TestCollectionsImmutable(IEnumerable<TKey> keys, IEnumerable<TValue> values)
  {
    var keysList = keys.ToList();
    var valuesList = values.ToList();

    var tempDict = ImmutableDictionary<TKey, TValue>.Empty.ToBuilder();
    for (int i = 0; i < keysList.Count; i++)
    {
      tempDict[keysList[i]] = valuesList[i];
    }

    keyList = keysList.ToImmutableList();
    keyValueDict = tempDict.ToImmutable();
  }

  public bool ContainsKey(TKey key)
  {
    return keyList.Contains(key); // Linear
  }

  public bool ContainsKeyInDict(TKey key)
  {
    return keyValueDict.ContainsKey(key); // O(log n)
  }
}


public class TestCollectionsSorted<TKey, TValue> where TKey : notnull
{
  private SortedList<TKey, TValue> sortedList;
  private SortedDictionary<TKey, TValue> sortedDict;

  public TestCollectionsSorted(IEnumerable<TKey> keys, IEnumerable<TValue> values)
  {
    sortedList = new SortedList<TKey, TValue>();
    sortedDict = new SortedDictionary<TKey, TValue>();

    var keyArray = keys.ToArray();
    var valueArray = values.ToArray();

    for (int i = 0; i < keyArray.Length; i++)
    {
      sortedList[keyArray[i]] = valueArray[i];
      sortedDict[keyArray[i]] = valueArray[i];
    }
  }

  public bool ContainsKeyInList(TKey key)
  {
    return sortedList.ContainsKey(key); // O(log n)
  }

  public bool ContainsKeyInDict(TKey key)
  {
    return sortedDict.ContainsKey(key); // O(log n)
  }
}




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

public class Person : IComparable<Person>, IComparer<Person>
{
  public string Name { get; init; }
  public string Surname { get; init; }
  public DateTime BirthDate { get; init; }

  // Порівняння по прізвищу
  public int CompareTo(Person other)
  {
    return this.Surname.CompareTo(other.Surname);
  }

  // Порівняння по даті народження
  public int Compare(Person x, Person y)
  {
    return x.BirthDate.CompareTo(y.BirthDate);
  }

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

  public SortedList<int, Student> Students { get; set; } = new SortedList<int, Student>();

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

  public Person BaseKey => new(Name, Surname, BirthDate);

  public Person GetBaseKey() => new(Name, Surname, BirthDate);

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

public class StudentCollection
{
  private List<Student> students = new();

  public void AddDefaults()
  {
    students.Add(new Student(new Person("Default", "Student", new DateTime(2000, 1, 1)), Education.Bachelor, 101));
  }

  public void AddStudents(params Student[] newStudents)
  {
    students.AddRange(newStudents);
  }

  public void SortBySurname()
  {
    students.Sort(); // використовує IComparable
  }

  public void SortByDate()
  {
    students.Sort((s1, s2) => s1.BirthDate.CompareTo(s2.BirthDate));
  }

  public void SortByAverageMark()
  {
    students.Sort((s1, s2) => s1.AverageGrade.CompareTo(s2.AverageGrade));
  }

  public double MaxAverageMark => students.Count == 0 ? 0 : students.Max(s => s.AverageGrade);

  public IEnumerable<Student> OnlyMasters => students.Where(s => s.Education == Education.Master);

  public IEnumerable<IGrouping<double, Student>> AverageMarkGroup(double value)
  {
    return students
        .Where(s => Math.Abs(s.AverageGrade - value) < 0.01) // точне порівняння з допуском
        .GroupBy(s => s.AverageGrade)
        .ToList();
  }

  public override string ToString()
  {
    return string.Join("\n", students.Select(s => s.ToShortString()));
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

    foreach (Test test1 in student.PassedTestsWithExam())
    {
      Console.WriteLine(test1);
    }

    Console.WriteLine("\nЛабораторна №3:");
    Console.OutputEncoding = System.Text.Encoding.UTF8;

    // --- 1. Створення об'єкта StudentCollection ---
    StudentCollection studentCollection = new StudentCollection();

    // --- 2. Додавання студентів ---
    studentCollection.AddDefaults(); // або вручну, якщо нема такого методу:
    studentCollection.AddStudents(
        new Student(new Person("Anna", "Zelenska", new DateTime(2001, 5, 21)), Education.Bachelor, 80),
        new Student(new Person("Bohdan", "Ivanchuk", new DateTime(1999, 11, 15)), Education.Master, 90),
        new Student(new Person("Oleksii", "Petrenko", new DateTime(2000, 3, 10)), Education.SecondEducation, 75)
    );

    Console.WriteLine(">>> Початковий список студентів:\n" + studentCollection);

    // --- 3. Сортування за прізвищем (IComparable) ---
    studentCollection.SortBySurname();
    Console.WriteLine(">>> Список після сортування за прізвищем:\n" + studentCollection);

    // --- 4. Сортування за датою народження (IComparer<Person>) ---
    studentCollection.SortByDate();
    Console.WriteLine(">>> Список після сортування за датою народження:\n" + studentCollection);

    // --- 5. Сортування за середнім балом (IComparer<Student>) ---
    studentCollection.SortByAverageMark();
    Console.WriteLine(">>> Список після сортування за середнім балом:\n" + studentCollection);

    // --- 6. Обчислення максимального середнього балу ---
    double maxAvg = studentCollection.MaxAverageMark;
    Console.WriteLine($">>> Максимальний середній бал: {maxAvg}");

    // --- 7. Фільтрація студентів з формою навчання Master ---
    var masters = studentCollection.OnlyMasters;
    Console.WriteLine(">>> Студенти з формою навчання Master:");
    foreach (var student1 in masters)
      Console.WriteLine(student1);

    // --- 8. Групування за середнім балом ---
    Console.WriteLine(">>> Групи студентів за середнім балом:");
    foreach (var group in studentCollection.AverageMarkGroup(80))
    {
      Console.WriteLine($"Середній бал: {group.Key}");
      foreach (var student2 in group)
        Console.WriteLine(student2);
    }

    // --- 9. Тестування TestCollections ---
    Console.WriteLine("\n>>> Тестування TestCollections:");
    var testCollections = new TestCollections<Person, Student>(
    10,
    i =>
    {
      var person = new Person($"Name{i}", $"Surname{i}", new DateTime(2000, 1, 1).AddDays(i));
      var student = new Student(person, Education.Bachelor, 101);
      return (person, student);
    }
);

    testCollections.TestSearchAll(0);  // перший елемент
    testCollections.TestSearchAll(5);  // центральний елемент
    testCollections.TestSearchAll(9);  // останній елемент
    testCollections.TestSearchAll(100); // неіснуючий елемент



    Console.WriteLine("\nЛабораторна №4:");
    int count = 10000;

    // Генерація тестових даних
    List<Person> people = new List<Person>();
    for (int i = 0; i < count; i++)
    {
      people.Add(new Person("Name" + i, "Surname" + i, new DateTime(1990, 1, 1).AddDays(i)));
    }

    // Ключі — Person, значення — Student
    var keys = people;
    var values = people.Select(p => new Student(p, Education.Bachelor, 101)).ToList();

    // ==== Стандартні колекції ====
    var sw = new Stopwatch();
    sw.Start();
    var standard = new TestCollections<Person, Student>(
        count,  // The number of elements
        i =>    // A function to generate a tuple (Person, Student)
        {
          var person = new Person($"Name{i}", $"Surname{i}", new DateTime(2000, 1, 1).AddDays(i));
          var student = new Student(person, Education.Bachelor, 101);
          return (person, student);  // Return a tuple (key, value)
        }
    );
    sw.Stop();
    Console.WriteLine($"Standard: додавання {count} елементів = {sw.ElapsedMilliseconds} мс");

    // Accessing values from the DictKey dictionary
    sw.Restart();
    Console.WriteLine("Standard: Contains (перший): " + standard.DictKey.Values.Contains(values[0]));
    sw.Stop();
    Console.WriteLine($"Standard: пошук першого = {sw.ElapsedTicks} тік");

    // ==== Immutable ====
    sw.Restart();
    var immutable = new TestCollectionsImmutable<Person, string>(
        keys,
        values.Select(v => v.ToString()).ToList() // конвертація Student -> string
    );
    sw.Stop();
    Console.WriteLine($"Immutable: додавання {count} елементів = {sw.ElapsedMilliseconds} мс");

    sw.Restart();
    Console.WriteLine("Immutable: ContainsKey (перший): " + immutable.ContainsKey(keys[0]));
    sw.Stop();
    Console.WriteLine($"Immutable: пошук першого = {sw.ElapsedTicks} тік");

    // ==== Sorted ====
    sw.Restart();
    var sorted = new TestCollectionsSorted<Person, string>(
        keys,
        values.Select(v => v.ToString()).ToList()
    );
    sw.Stop();
    Console.WriteLine($"Sorted: додавання {count} елементів = {sw.ElapsedMilliseconds} мс");

    sw.Restart();
    Console.WriteLine("Sorted: ContainsKey (перший): " + sorted.ContainsKeyInList(keys[0]));
    sw.Stop();
    Console.WriteLine($"Sorted: пошук першого = {sw.ElapsedTicks} тік");

  }
}
