﻿using System;
using System.Linq;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using Newtonsoft.Json;




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
    ListValue = new List<TValue>(size);  // setialize the ListValue to store TValue data

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
  DateTime Date { get; set; }
  object DeepCopy();
}

public enum Education
{
  Master,
  Bachelor,
  SecondEducation
}

[Serializable]
public class Exam
{
  public string Subject { get; set; }
  public int Grade { get; set; }
  public DateTime ExamDate { get; set; }

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

[Serializable]
public class Person : IComparable<Person>, IComparer<Person>
{
  public string Name { get; set; }
  public string Surname { get; set; }
  public DateTime BirthDate { get; set; }

  public int CompareTo(Person? other)
  {
    if (other == null) return 1;

    int result = Surname.CompareTo(other.Surname);
    if (result != 0) return result;

    result = Name.CompareTo(other.Name);
    if (result != 0) return result;

    return BirthDate.CompareTo(other.BirthDate);
  }


  public int Compare(Person? x, Person? y)
  {
    if (ReferenceEquals(x, y)) return 0;
    if (x is null) return -1;
    if (y is null) return 1;
    return x.CompareTo(y);
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

[Serializable]
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

[JsonObject]
public class Student : Person
{
    private Education _education;
    private int _group;

    private List<Test> _tests = new List<Test>();
    private List<Exam> _exams = new List<Exam>();

    public DateTime Date { get; private set; }

    public Student(Person person, Education education, int group)
        : base(person.Name, person.Surname, person.BirthDate)
    {
        Education = education;
        Group = group;
    }

    public Student() : this(new Person(), Education.Bachelor, 101) { }

    public Student(Student temp) : base(temp.Name, temp.Surname, temp.BirthDate)
    {
        Education = temp.Education;
        Group = temp.Group;
        Exams = new List<Exam>(temp.Exams ?? new List<Exam>());
        Tests = new List<Test>(temp.Tests ?? new List<Test>());
        Group = 100;
    }

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
            bool hasExam = _exams.Any(e => e.Subject == test.Subject && e.Grade > 2);
            if (hasExam)
                yield return test;
        }
    }

    public Person PersonalInfo => this;

    public Education Education
    {
        get => _education;
        set => _education = value;
    }

    public int Group
    {
        get => _group;
        set
        {
            if (value < 100 || value > 699)
                throw new ArgumentOutOfRangeException(nameof(Group), $"Група має бути в межах від 100 до 699. Отримано: {value}");
            _group = value;
        }
    }

    public List<Exam> Exams
    {
        get => _exams;
        set => _exams = value ?? new List<Exam>();
    }

    public List<Test> Tests
    {
        get => _tests;
        set => _tests = value ?? new List<Test>();
    }

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

    public void AddExams(params Exam[] exams)
    {
        if (exams == null || exams.Length == 0)
            return;
        Exams.AddRange(exams);
    }

    public void AddTests(params Test[] tests)
    {
        if (tests == null || tests.Length == 0)
            return;
        Tests.AddRange(tests);
    }

    public override string ToString()
    {
        string examsInfo = Exams.Count > 0 ? string.Join("; ", Exams.Select(e => e.ToString())) : "No exams";
        string testsInfo = Tests.Count > 0 ? string.Join("; ", Tests.Select(t => t.ToString())) : "No tests";
        return $"{base.ToString()}, Education: {_education}, Group: {_group}, Exams: [{examsInfo}], Tests: [{testsInfo}]";
    }

    public override string ToShortString()
    {
        return $"{base.ToString()}, Education: {Education}, Group: {Group}, Avg. Grade: {AverageGrade:F2}";
    }

    public bool this[Education edu] => Education == edu;

    public object DeepCopy()
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        var json = JsonConvert.SerializeObject(this, settings);
        var copy = JsonConvert.DeserializeObject<Student>(json, settings);
        return copy;
    }

    public bool Save(string filename)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filename, json);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving file: {ex.Message}");
            return false;
        }
    }

    public bool Load(string filename)
    {
        try
        {
            if (!File.Exists(filename))
                return false;

            var json = File.ReadAllText(filename);
            var obj = System.Text.Json.JsonSerializer.Deserialize<Student>(json);

            if (obj != null)
            {
                Exams = obj._exams;
                Tests = obj._tests;
                Education = obj.Education;
                Group = obj.Group;
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading file: {ex.Message}");
            return false;
        }
    }

    public static bool Save(string filename, Student obj)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(obj);
            File.WriteAllText(filename, json);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Static save error: {ex.Message}");
            return false;
        }
    }

    public static bool Load(string filename, Student obj)
    {
        try
        {
            if (!File.Exists(filename))
                return false;

            var json = File.ReadAllText(filename);
            var newObj = System.Text.Json.JsonSerializer.Deserialize<Student>(json);

            if (newObj != null)
            {
                obj._exams = newObj._exams;
                obj._tests = newObj._tests;
                obj.Education = newObj.Education;
                obj.Group = newObj.Group;
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Static load error: {ex.Message}");
            return false;
        }
    }

    public bool AddFromConsole()
    {
        Console.WriteLine("Введіть дані екзамену у форматі: Назва_предмету,Оцінка(ціле число),Дата(дд.мм.рррр)");
        Console.WriteLine("Роздільники: кома ','");

        string input = Console.ReadLine();
        string[] parts = input?.Split(',');

        if (parts == null || parts.Length != 3)
        {
            Console.WriteLine("Неправильний формат введення.");
            return false;
        }

        try
        {
            string subject = parts[0].Trim();
            int grade = int.Parse(parts[1].Trim());
            DateTime date = DateTime.ParseExact(parts[2].Trim(), "dd.MM.yyyy", null);

            var exam = new Exam(subject, grade, date);
            _exams.Add(exam);

            Console.WriteLine("Екзамен успішно доданий.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка при парсингу: {ex.Message}");
            return false;
        }
    }
}

public delegate void StudentListHandler(object source, StudentListHandlerEventArgs args);

public class StudentCollection
{
  private List<Student> students = new();

  public required string CollectionName { get; set; }

  public event StudentListHandler? StudentCountChanged;
  public event StudentListHandler? StudentReferenceChanged;

  protected virtual void OnStudentCountChanged(StudentListHandlerEventArgs args)
  {
    StudentCountChanged?.Invoke(this, args);
  }

  protected virtual void OnStudentReferenceChanged(StudentListHandlerEventArgs args)
  {
    StudentReferenceChanged?.Invoke(this, args);
  }

  public void AddDefaults()
  {
    var student = new Student(new Person("Default", "Student", new DateTime(2000, 1, 1)), Education.Bachelor, 101);
    students.Add(student);
    OnStudentCountChanged(new StudentListHandlerEventArgs(CollectionName, "Додано стандартного студента", student));
  }

  public void AddStudents(params Student[] newStudents)
  {
    foreach (var student in newStudents)
    {
      students.Add(student);
      OnStudentCountChanged(new StudentListHandlerEventArgs(CollectionName, "Додано нового студента", student));
    }
  }

  public bool Remove(int j)
  {
    if (j >= 0 && j < students.Count)
    {
      var removedStudent = students[j];
      students.RemoveAt(j);
      OnStudentCountChanged(new StudentListHandlerEventArgs(CollectionName, "Видалено студента", removedStudent));
      return true;
    }
    return false;
  }

  public Student this[int index]
  {
    get => students[index];
    set
    {
      if (index >= 0 && index < students.Count)
      {
        var oldStudent = students[index];
        students[index] = value;
        StudentReferenceChanged?.Invoke(this,
            new StudentListHandlerEventArgs(CollectionName, $"Замінено студента з індексом {index}", value));
      }
    }
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
        .Where(s => Math.Abs(s.AverageGrade - value) < 0.01)
        .GroupBy(s => s.AverageGrade)
        .ToList();
  }

  public override string ToString()
  {
    return string.Join("\n", students.Select(s => s.ToShortString()));
  }
}

public class StudentListHandlerEventArgs : EventArgs
{
  public string CollectionName { get; }
  public string ChangeType { get; }
  public Student ChangedStudent { get; }

  public StudentListHandlerEventArgs(string collectionName, string changeType, Student student)
  {
    CollectionName = collectionName;
    ChangeType = changeType;
    ChangedStudent = student;
  }

  public override string ToString()
  {
    return $"Collection: {CollectionName}, Change: {ChangeType}, Student: {ChangedStudent}";
  }
}

public class Journal
{
  private List<JournalEntry> entries = new();

  public void StudentEventHandler(object source, StudentListHandlerEventArgs args)
  {
    entries.Add(new JournalEntry(args.CollectionName, args.ChangeType, args.ChangedStudent));
  }

  public override string ToString()
  {
    return string.Join("\n", entries);
  }
}


public class JournalEntry
{
  public string CollectionName { get; set; }
  public string ChangeType { get; set; }
  public Student ChangedStudent { get; set; }

  public JournalEntry(string collectionName, string changeType, Student student)
  {
    CollectionName = collectionName;
    ChangeType = changeType;
    ChangedStudent = student;
  }

  public override string ToString()
  {
    return $"[Журнал] Колекція: {CollectionName}, Тип зміни: {ChangeType}, Студент: {ChangedStudent.ToShortString()}";
  }
}


class Program
{
  static void Main()
  {


    Console.WriteLine("\nЛабораторна №5:");

    // Створення колекцій
    var collection1 = new StudentCollection { CollectionName = "Група 1" };
    var collection2 = new StudentCollection { CollectionName = "Група 2" };

    // Створення журналів
    var journal1 = new Journal();
    var journal2 = new Journal();

    // Підписка на події
    collection1.StudentCountChanged += journal1.StudentEventHandler;
    collection1.StudentReferenceChanged += journal1.StudentEventHandler;

    collection2.StudentReferenceChanged += journal2.StudentEventHandler;

    // Дії з колекціями
    var student1 = new Student(new Person("Анна", "Іваненко", new DateTime(2001, 3, 14)), Education.Bachelor, 101);
    var student2 = new Student(new Person("Богдан", "Коваленко", new DateTime(2000, 6, 5)), Education.Master, 202);

    collection1.AddStudents(student1, student2);
    collection1.Remove(0);
    collection2.AddDefaults();

    // Заміна студента в collection2
    collection2[0] = new Student(new Person("Ірина", "Сидоренко", new DateTime(2002, 9, 23)), Education.SecondEducation, 303);

    // Виведення журналів
    Console.WriteLine("=== Журнал 1 ===");
    Console.WriteLine(journal1);

    Console.WriteLine("\n=== Журнал 2 ===");
    Console.WriteLine(journal2);

    Console.WriteLine("\nЛабораторна №6:");

    Student student = new Student();

    Console.WriteLine("Додаємо екзамен через консоль:");
    bool added = student.AddFromConsole();

    if (added)
    {
      Console.WriteLine("Поточний список екзаменів студента:");
      foreach (var exam in student.Exams)
      {
        Console.WriteLine(exam);
      }
    }
    else
    {
      Console.WriteLine("Не вдалося додати екзамен.");
    }

    string filename = "student_data.json";

    Console.WriteLine($"\nЗбереження даних студента у файл '{filename}'...");
    bool saved = student.Save(filename);
    Console.WriteLine(saved ? "Збереження пройшло успішно." : "Помилка під час збереження.");

    Console.WriteLine("\nСтворюємо глибоку копію студента...");
    Student copy = (Student)student.DeepCopy();

    Console.WriteLine("Копія студента створена. Екзамени копії:");
    foreach (var exam in copy.Exams)
    {
      Console.WriteLine(exam);
    }

    Console.WriteLine("\nЗавантажуємо дані з файлу у нового студента...");
    Student loadedStudent = new Student();
    bool loaded = loadedStudent.Load(filename);
    Console.WriteLine(loaded ? "Завантаження пройшло успішно." : "Помилка при завантаженні.");

    Console.WriteLine("Екзамени завантаженого студента:");
    foreach (var exam in loadedStudent.Exams)
    {
      Console.WriteLine(exam);
    }

  }
















  //     Console.WriteLine("Введiть цiле число рядкiв, а потiм стовпцiв, роздiлених пробiлом, комою або крапкою з комою:");

  //     string? inputLine = Console.ReadLine();
  //     if (string.IsNullOrWhiteSpace(inputLine))
  //     {
  //       Console.WriteLine("Помилка! Введіть два додатних цілих числа.");
  //       return;
  //     }

  //     string[] input = inputLine.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
  //     if (input.Length < 2 || !int.TryParse(input[0], out int nRows) || !int.TryParse(input[1], out int nColumns) || nRows <= 0 || nColumns <= 0)
  //     {
  //       Console.WriteLine("Помилка! Введіть два додатних цілих числа.");
  //       return;
  //     }

  //     var nRowsColumns = nRows * nColumns;
  //     var oneDimensional = new Exam[nRowsColumns];
  //     var twoDimensional = new Exam[nRows, nColumns];

  //     var increasingJaggedArray = new Exam[nRows][];

  //     int acc = 0, rows = 0;
  //     while (acc < nRowsColumns)
  //     {
  //       rows++;
  //       acc += rows;
  //     }
  //     var jaggedArray = new Exam[rows][];

  //     for (int i = 0; i < rows - 1; i++)
  //       jaggedArray[i] = new Exam[i + 1];

  //     jaggedArray[rows - 1] = new Exam[rows - (acc - nRowsColumns)];

  //     int assignedElements = 0;
  //     for (int i = 0; i < nRows; i++)
  //     {
  //       int remaining = (nRows * nColumns) - assignedElements;
  //       int rowSize = Math.Min(nColumns, remaining);
  //       increasingJaggedArray[i] = new Exam[rowSize];
  //       assignedElements += rowSize;
  //     }

  //     for (int i = 0; i < nRowsColumns; i++)
  //     {
  //       oneDimensional[i] = new Exam("Math", 0, DateTime.Now);
  //     }

  //     for (int i = 0; i < nRows; i++)
  //     {
  //       for (int j = 0; j < nColumns; j++)
  //       {
  //         twoDimensional[i, j] = new Exam("Math", 0, DateTime.Now);
  //       }
  //     }

  //     for (int i = 0; i < jaggedArray.Length; i++)
  //     {
  //       for (int j = 0; j < jaggedArray[i].Length; j++)
  //       {
  //         jaggedArray[i][j] = new Exam("Math", 0, DateTime.Now);
  //       }
  //     }

  //     int startTime = Environment.TickCount;
  //     for (int i = 0; i < nRows * nColumns; i++)
  //     {
  //       oneDimensional[i].Grade = 100;
  //     }
  //     int oneDimTime = Environment.TickCount - startTime;

  //     startTime = Environment.TickCount;
  //     for (int i = 0; i < nRows; i++)
  //     {
  //       for (int j = 0; j < nColumns; j++)
  //       {
  //         twoDimensional[i, j].Grade = 100;
  //       }
  //     }
  //     int twoDimTime = Environment.TickCount - startTime;

  //     startTime = Environment.TickCount;
  //     for (int i = 0; i < jaggedArray.Length; i++)
  //     {
  //       for (int j = 0; j < jaggedArray[i].Length; j++)
  //       {
  //         jaggedArray[i][j].Grade = 100;
  //       }
  //     }
  //     int jaggedTime = Environment.TickCount - startTime;

  //     Console.WriteLine("\nЧас виконання:");
  //     Console.WriteLine($"1-вимiрний: {oneDimTime} мс");
  //     Console.WriteLine($"2-вимiрний: {twoDimTime} мс");
  //     Console.WriteLine($"Зубчатий: {jaggedTime} мс");

  //     var person = new Person("Iван", "Петров", new DateTime(2002, 5, 12));
  //     var student = new Student(person, Education.Bachelor, 101);

  //     Console.WriteLine("\nToShortString:");
  //     Console.WriteLine(student.ToShortString());

  //     Console.WriteLine("\nIндекс:");
  //     Console.WriteLine($"Master: {student[Education.Master]}");
  //     Console.WriteLine($"Bachelor: {student[Education.Bachelor]}");
  //     Console.WriteLine($"SecondEducation: {student[Education.SecondEducation]}");

  //     Console.WriteLine("\nToString:");
  //     Console.WriteLine(student.ToString());

  //     var exam1 = new Exam("Physics", 85, new DateTime(2024, 6, 1));
  //     var exam2 = new Exam("Chemistry", 90, new DateTime(2024, 6, 5));
  //     student.AddExams(exam1, exam2);

  //     student.AddTests(
  //     new Test("Math", true),
  //     new Test("OOP", false)
  // );

  //     Console.WriteLine("\nПiсля додавання екзаменiв:");
  //     Console.WriteLine(student.ToString());

  //     Console.WriteLine("\nЛабораторна робота №2:\n");

  //     var p1 = new Person("Олег", "Ковальчук", new DateTime(2000, 1, 1));
  //     var p2 = new Person("Олег", "Ковальчук", new DateTime(2000, 1, 1));
  //     var p3 = new Person("Інна", "Ковальчук", new DateTime(2000, 1, 1));




  //     Console.WriteLine($"p1 == p2: {p1 == p2}"); // true
  //     Console.WriteLine($"p1 != p3: {p1 != p3}"); // true
  //     Console.WriteLine($"p1.Equals(p2): {p1.Equals(p2)}"); // true
  //     Console.WriteLine($"HashCode p1: {p1.GetHashCode()} | p2: {p2.GetHashCode()}"); // однакові

  //     Console.WriteLine("\nВластивості типу Person для об'єкта Student:");
  //     Console.WriteLine(student.PersonalInfo);

  //     Console.WriteLine("\nКопiя студента (до змiни оригіналу):");
  //     var studentCopy = (Student)student.DeepCopy();
  //     Console.WriteLine(studentCopy);

  //     // Змінюємо оригінал
  //     student.Exams[0] = new Exam("Changed", 20, DateTime.Today);

  //     Console.WriteLine("\nОригiнальний студент (пiсля змiни):");
  //     Console.WriteLine(student);
  //     Console.WriteLine("\nКопiя студента (має залишитись без змiн):");
  //     Console.WriteLine(studentCopy);

  //     try
  //     {
  //       var brokenStudent = new Student(new Person(), Education.Master, 50); // некоректне значення
  //     }
  //     catch (ArgumentOutOfRangeException ex)
  //     {
  //       Console.WriteLine($"\nПомилка: {ex.Message}");
  //     }

  //     Console.WriteLine("\nЕкзамени з оцiнкою > 3:");
  //     foreach (Exam exam in student.GetExamsAbove(3))
  //     {
  //       Console.WriteLine(exam);
  //     }
  //     Console.WriteLine("\nУнікальні предмети (StudentEnumerator):");
  //     foreach (string subject in student)
  //     {
  //       Console.WriteLine(subject);
  //     }

  //     foreach (object item in student.PassedItems())
  //     {
  //       Console.WriteLine(item);
  //     }

  //     foreach (Test test1 in student.PassedTestsWithExam())
  //     {
  //       Console.WriteLine(test1);
  //     }

  //     Console.WriteLine("\nЛабораторна №3:");
  //     Console.OutputEncoding = System.Text.Encoding.UTF8;

  //     // --- 1. Створення об'єкта StudentCollection ---
  //     StudentCollection studentCollection = new StudentCollection();

  //     // --- 2. Додавання студентів ---
  //     studentCollection.AddDefaults(); // або вручну, якщо нема такого методу:
  //     studentCollection.AddStudents(
  //         new Student(new Person("Anna", "Zelenska", new DateTime(2001, 5, 21)), Education.Bachelor, 80),
  //         new Student(new Person("Bohdan", "Ivanchuk", new DateTime(1999, 11, 15)), Education.Master, 90),
  //         new Student(new Person("Oleksii", "Petrenko", new DateTime(2000, 3, 10)), Education.SecondEducation, 75)
  //     );

  //     Console.WriteLine(">>> Початковий список студентів:\n" + studentCollection);

  //     // --- 3. Сортування за прізвищем (IComparable) ---
  //     studentCollection.SortBySurname();
  //     Console.WriteLine(">>> Список після сортування за прізвищем:\n" + studentCollection);

  //     // --- 4. Сортування за датою народження (IComparer<Person>) ---
  //     studentCollection.SortByDate();
  //     Console.WriteLine(">>> Список після сортування за датою народження:\n" + studentCollection);

  //     // --- 5. Сортування за середнім балом (IComparer<Student>) ---
  //     studentCollection.SortByAverageMark();
  //     Console.WriteLine(">>> Список після сортування за середнім балом:\n" + studentCollection);

  //     // --- 6. Обчислення максимального середнього балу ---
  //     double maxAvg = studentCollection.MaxAverageMark;
  //     Console.WriteLine($">>> Максимальний середній бал: {maxAvg}");

  //     // --- 7. Фільтрація студентів з формою навчання Master ---
  //     var masters = studentCollection.OnlyMasters;
  //     Console.WriteLine(">>> Студенти з формою навчання Master:");
  //     foreach (var student1 in masters)
  //       Console.WriteLine(student1);

  //     // --- 8. Групування за середнім балом ---
  //     Console.WriteLine(">>> Групи студентів за середнім балом:");
  //     foreach (var group in studentCollection.AverageMarkGroup(80))
  //     {
  //       Console.WriteLine($"Середній бал: {group.Key}");
  //       foreach (var student2 in group)
  //         Console.WriteLine(student2);
  //     }

  //     // --- 9. Тестування TestCollections ---
  //     Console.WriteLine("\n>>> Тестування TestCollections:");
  //     var testCollections = new TestCollections<Person, Student>(
  //     10,
  //     i =>
  //     {
  //       var person = new Person($"Name{i}", $"Surname{i}", new DateTime(2000, 1, 1).AddDays(i));
  //       var student = new Student(person, Education.Bachelor, 101);
  //       return (person, student);
  //     }
  // );

  //     testCollections.TestSearchAll(0);  // перший елемент
  //     testCollections.TestSearchAll(5);  // центральний елемент
  //     testCollections.TestSearchAll(9);  // останній елемент
  //     testCollections.TestSearchAll(100); // неіснуючий елемент



  //     Console.WriteLine("\nЛабораторна №4:");
  //     int count = 10000;

  //     // Генерація тестових даних
  //     List<Person> people = new List<Person>();
  //     for (int i = 0; i < count; i++)
  //     {
  //       people.Add(new Person("Name" + i, "Surname" + i, new DateTime(1990, 1, 1).AddDays(i)));
  //     }

  //     // Ключі — Person, значення — Student
  //     var keys = people;
  //     var values = people.Select(p => new Student(p, Education.Bachelor, 101)).ToList();

  //     // ==== Стандартні колекції ====
  //     var sw = new Stopwatch();
  //     sw.Start();
  //     var standard = new TestCollections<Person, Student>(
  //         count,  // The number of elements
  //         i =>    // A function to generate a tuple (Person, Student)
  //         {
  //           var person = new Person($"Name{i}", $"Surname{i}", new DateTime(2000, 1, 1).AddDays(i));
  //           var student = new Student(person, Education.Bachelor, 101);
  //           return (person, student);  // Return a tuple (key, value)
  //         }
  //     );
  //     sw.Stop();
  //     Console.WriteLine($"Standard: додавання {count} елементів = {sw.ElapsedMilliseconds} мс");

  //     // Accessing values from the DictKey dictionary
  //     sw.Restart();
  //     Console.WriteLine("Standard: Contains (перший): " + standard.DictKey.Values.Contains(values[0]));
  //     sw.Stop();
  //     Console.WriteLine($"Standard: пошук першого = {sw.ElapsedTicks} тік");

  //     // ==== Immutable ====
  //     sw.Restart();
  //     var immutable = new TestCollectionsImmutable<Person, string>(
  //         keys,
  //         values.Select(v => v.ToString()).ToList() // конвертація Student -> string
  //     );
  //     sw.Stop();
  //     Console.WriteLine($"Immutable: додавання {count} елементів = {sw.ElapsedMilliseconds} мс");

  //     sw.Restart();
  //     Console.WriteLine("Immutable: ContainsKey (перший): " + immutable.ContainsKey(keys[0]));
  //     sw.Stop();
  //     Console.WriteLine($"Immutable: пошук першого = {sw.ElapsedTicks} тік");

  //     // ==== Sorted ====
  //     sw.Restart();
  //     var sorted = new TestCollectionsSorted<Person, string>(
  //         keys,
  //         values.Select(v => v.ToString()).ToList()
  //     );
  //     sw.Stop();
  //     Console.WriteLine($"Sorted: додавання {count} елементів = {sw.ElapsedMilliseconds} мс");

  //     sw.Restart();
  //     Console.WriteLine("Sorted: ContainsKey (перший): " + sorted.ContainsKeyInList(keys[0]));
  //     sw.Stop();
  //     Console.WriteLine($"Sorted: пошук першого = {sw.ElapsedTicks} тік");



}
