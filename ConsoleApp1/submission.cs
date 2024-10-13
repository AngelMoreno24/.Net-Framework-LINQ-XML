using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using System.IO;
using System.Linq;
using System.Collections;
using System.Text.RegularExpressions;
namespace ConsoleApp1
{
    public class Program
    {
        public static void Main(String[] args)
        {
            // Question 1.1 Main() function
            Console.WriteLine("---- Q1.1 Main() function and read file into course list");
            // Question 1.2 Question 1.2 Define Course class and read course file into course list
            Course[] coursesList = readCourseFile("Courses.csv"); // Read Excel file into a list

            // Question 1.3a Query course by subject, level
            var results = qCourses(coursesList, "IEE", 300);
            foreach (var result in results)
            {
                Console.WriteLine($"Course Title: {result.Title} - Instructor: {result.Instructor}");
            }

            // Question 1.3b Course by subject, code in groups, print courses with at least 2 courses
            var groups = qGroups(coursesList); // at least 2 courses
            int n = 2;
            foreach (var subjectGroup in groups)
            {
                var flag = true;
                foreach (var codeGroup in subjectGroup.Groups)
                {
                    if (codeGroup.Courses.Count >= n)
                    {
                        if (flag)
                        {
                            Console.WriteLine($"Subject: {subjectGroup.Subject}");
                            flag = false;
                        }
                        Console.WriteLine($"\tCode: {codeGroup.Code}");
                        foreach (var course in codeGroup.Courses)
                        {
                            Console.WriteLine($"\t\t{course.Title}");
                        }
                    }
                }
            }
            // Question 1.4 Create Instructor file manually and read the file into a list
            List<Instructor> instructorsList = readInstructorFile("Instructors.csv");
            Console.WriteLine(instructorsList.Count);
            // Question 1.5 Find instructor’s email address for each course
            var email_query_results = qNamesEmails(instructorsList, coursesList, 200);

            // Print all courses at the level = 200 - 299
            foreach (var result in email_query_results)
            {
                Console.WriteLine($"{result.Course} - {result.InstructorEmail}");
            }
            // Question 2.1a Retrieve CPI courses of number of 200 or higher
            coursesList = readCourseFile("Courses.csv"); // Read Excel file into a list
            var cpiCourses = qTitleInstructorInXml(coursesList);
            foreach (var course in cpiCourses)
            {
                Console.WriteLine(course);
            }
            // Question 2.1b Get courses in groups: subject 1st level key, code 2nd level key
            var courseGroups = qTitleCodeGroups(coursesList);
            Console.WriteLine(courseGroups);
            // Question 2.2 Deliver the result set in the type IEnumerable<XElement>
            var resultXML = qCourseInstructorEnum(instructorsList, coursesList);
            foreach (XElement courseElement in resultXML)
            {
                Console.WriteLine(courseElement);
            }
        }
        // Question 1.2 Read course file into course list
        public static Course[] readCourseFile(string fpath)
        {
            Console.WriteLine("---- Q1.2 Read file into course list");

            //list contains course objects and is used for the output
            List<Course> tmpList = new List<Course>();


            //reads courses file
            using (var textReader = new System.IO.StreamReader(fpath))
            { 
                //variable is used later for parsing
                char _Delimiter = ','; 

                //skips the first line
                string line = textReader.ReadLine();
                int skipCount = 0;

                //reads the second line
                while (line != null && skipCount < 1)
                { 
                    line = textReader.ReadLine();
                    skipCount++;
                }

                //count keeps track of the current amount of lines read
                int count = 0;
                while (line != null && count < 131)
                {
                    //stores each column
                    string[] columns = line.Split(_Delimiter);

                    //stores the course info if the amount of columns is correct
                    if (columns.Length == 11)
                    {
                        //stores a Course object containing info from the current row of the .csv file
                        tmpList.Add(new Course
                        {
                            SubjectCode = columns[0],
                            Title = columns[1],
                            CourseId = columns[2],
                            Location = columns[7],
                            Instructor = columns[3],
                        });
                        count++;
                    }
                    //iterates through each line
                    line = textReader.ReadLine();
                }

            }

            return tmpList.ToArray<Course>();
        }
        public static dynamic qCourses(Course[] coursesList, String subject, int level)
        {
            Console.WriteLine("---- Q1.3a Query course by subject, level");

            //stores the title and instructor of queried course types that are above the given level(300) and of the given subject(IEE)
            IEnumerable results =
                from b in coursesList
                where (b.getCode() >= level && b.getSubject() == subject)
                orderby b.Instructor
                select new
                {
                    Title = b.Title,
                    Instructor = b.Instructor
                };

            return results;
        }
        public static dynamic qGroups(Course[] coursesList)
        {
            Console.WriteLine("---- Q1.3b Course by subject, code in groups");

            //stores the grouped values of course subject as the first level key and course code as the second level key.
            var groups = from course in coursesList
                         group course by course.getSubject() into subjectGroup
                         select new
                         {
                             //1st level
                             Subject = subjectGroup.Key,
                             Groups = from courseGroup in subjectGroup
                                      group courseGroup by courseGroup.getCode() into codeGroup
                                      select new
                                      {
                                          //2nd level
                                          Code = codeGroup.Key,
                                          Courses = codeGroup.ToList()

                                      }
                         };

            return groups;
        }
        //Question 1.4 Create and read file into instructor list
        public static List<Instructor> readInstructorFile(string fpath)
        {
            Console.WriteLine("---- Q1.4 Read file into instructor list");

            //stores list of Instructo object for the output
            List<Instructor> tmpList = new List<Instructor>();

            //reaed the Instructor.csv file
            using (var textReader = new System.IO.StreamReader(fpath))
            { 
                //used to parse file 
                char _Delimiter = ',';
                
                //skips the first line
                string line = textReader.ReadLine();
               
                //reads the 2nd line in order to start there
                line = textReader.ReadLine();

                //loops through each row
                while (line != null)
                {
                    //parses the line into 3 columns
                    string[] columns = line.Split(_Delimiter);

                    //creates Instructor object with read values and adds it to the list
                    tmpList.Add(new Instructor
                    {
                        InstructorName = columns[0],
                        OfficeNumber = columns[1],
                        EmailAddress = columns[2],
                    });

                    line = textReader.ReadLine();
                }

            }
            return tmpList;
        }
        public static dynamic qNamesEmails(List<Instructor> instructorsList, Course[] coursesList, int level)
        {
            Console.WriteLine("---- Q1.5 Find instructor’s email address for each course");

            //stores the subjectcode and email of instructors whose course level is between 200 and 299
            var query = from course in coursesList
                        where course.getCode() >= level && course.getCode() <= level + 99
                        join instructor in instructorsList on course.Instructor equals instructor.InstructorName
                        orderby course.SubjectCode
                        select new
                        {
                            Course = course.SubjectCode,
                            InstructorEmail = instructor.EmailAddress
                        };

            return query;
        }
        public static dynamic qTitleInstructorInXml(Course[] coursesList)
        {
            Console.WriteLine("---- Q2.1a Retrieve CPI courses of number of 200 or higher");

            //Creates an Enumerable<XElement> containing the title and instructor for CPI courses of level 200 or higher. sorted by instructor in ascending order
            IEnumerable<XElement> cpiCourses =
                from course in coursesList
                where course.getSubject() == "CPI" && course.getCode() >= 200
                orderby course.Instructor
                select new XElement("Course",
                    new XElement("Title", course.Title),
                    new XElement("Instructor", course.Instructor)
                    );

            return cpiCourses;
        }
        public static dynamic qTitleCodeGroups(Course[] coursesList)
        {
            Console.WriteLine("---- Q2.1b Get courses in groups: subject 1st level key, code 2nd level key");

            //query the subject and code for courses in 2 levels.  
            var courseGroups2 =
                            from course in coursesList
                            group course by course.getSubject() into subjectGroup
                            select new
                            {
                                Subject = subjectGroup.Key,
                                Count = subjectGroup.Count(),
                                Groups = from courseGroup in subjectGroup
                                         group courseGroup by courseGroup.getCode() into codeGroup
                                         select codeGroup
                            };


            //converts the LINQ object to an XElement type
            XElement courseGroups = new XElement("Courses",
                from group1 in courseGroups2
                select new XElement("Subject",
                    new XAttribute("Name", group1.Subject),
                    from group2 in group1.Groups
                    where group2.Count() >= 2   //only gets the groups with 2 courses in the 2nd level group 
                    select new XElement("Code",
                        new XAttribute("Number", group2.Key),
                    from customer in group2
                    select new XElement("Course", customer.Title)
                    )
                )
            );
            return courseGroups;
        }
        // Question 2.2 Deliver the result set in the type IEnumerable<XElement>
        public static dynamic qCourseInstructorEnum(List<Instructor> instructorsList, Course[] coursesList)
        {
            Console.WriteLine("---- Q2.2 Deliver the result set in the type IEnumerable<XElement>");


            //stores the courseCode, subject, and email for each course
            var query = from course in coursesList
                        where course.getCode() >= 200 && course.getCode() < 300
                        join instructor in instructorsList on course.Instructor equals instructor.InstructorName
                        orderby course.SubjectCode
                        select new
                        {
                            CourseCode = course.SubjectCode,
                            Subject = course.getSubject(),
                            InstructorEmail = instructor.EmailAddress
                        };

            //creates a list to store each seperate xml
            List<XElement> resultXMLList = new List<XElement>();

            //creates each individual xml using the performed query data and stores it into the previously created list
            foreach (var group1 in query)
            {
                resultXMLList.Add(new XElement("Course",
                    new XElement("Subject", group1.Subject),
                    new XElement("CourseCode", group1.CourseCode),
                    new XElement("InstructorEmail", group1.InstructorEmail)
                    )
                );
                
            }

            //Converts the list to an IEnumerable<XElement>
            IEnumerable<XElement> resultXML = resultXMLList;

            return resultXML;
        }
    }
    public class Course // Question 1.2
    {
        public string SubjectCode { get; set; }
        public string Title { get; set; }
        public string CourseId { get; set; }
        public string Location { get; set; }
        public string Instructor { get; set; }


        //sets the SubjectCode
        public void setSubjectCode(string subjectCode)
        {
            this.SubjectCode = subjectCode;

        }

        //gets the code without the subject included
        public int getCode()
        {
            int code = Int32.Parse((this.SubjectCode).Remove(0, 4));
            return code;
        }

        //gets the subject without the code included
        public string getSubject()
        {
            string subject = (this.SubjectCode).Remove(3, 4);
            return subject;
        }

    }
    public class Instructor // Question 1.4
    {
        public string InstructorName { get; set; }
        public string OfficeNumber { get; set; }
        public string EmailAddress { get; set; }

    }
}
