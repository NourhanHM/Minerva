using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Minerva.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminTb",
                columns: table => new
                {
                    Admin_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminTb", x => x.Admin_id);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentTb",
                columns: table => new
                {
                    Assignment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelAnswer = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    AssignmentFile = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Total_grade = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject_id = table.Column<int>(type: "int", nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Doctor_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentTb", x => x.Assignment_id);
                });

            migrationBuilder.CreateTable(
                name: "DoctorTb",
                columns: table => new
                {
                    Doctor_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorTb", x => x.Doctor_id);
                });

            migrationBuilder.CreateTable(
                name: "LectureTb",
                columns: table => new
                {
                    Lec_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Upload_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Subject_id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Doctor_id = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LectureTb", x => x.Lec_id);
                });

            migrationBuilder.CreateTable(
                name: "StudentTb",
                columns: table => new
                {
                    Student_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    National_id = table.Column<int>(type: "int", nullable: false),
                    Major = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTb", x => x.Student_id);
                });

            migrationBuilder.CreateTable(
                name: "SubjectTb",
                columns: table => new
                {
                    Subject_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Doctor_id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Student_ids = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectTb", x => x.Subject_id);
                });

            migrationBuilder.CreateTable(
                name: "UniversityTb",
                columns: table => new
                {
                    Uni_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversityTb", x => x.Uni_id);
                });

            migrationBuilder.CreateTable(
                name: "AttempTb",
                columns: table => new
                {
                    Attemp_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Student_id = table.Column<int>(type: "int", nullable: false),
                    Assignment_id = table.Column<int>(type: "int", nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Grade = table.Column<double>(type: "float", nullable: false),
                    Answer = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Enrollment_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttempTb", x => x.Attemp_id);
                    table.ForeignKey(
                        name: "FK_AttempTb_AssignmentTb_Assignment_id",
                        column: x => x.Assignment_id,
                        principalTable: "AssignmentTb",
                        principalColumn: "Assignment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttempTb_StudentTb_Student_id",
                        column: x => x.Student_id,
                        principalTable: "StudentTb",
                        principalColumn: "Student_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizTb",
                columns: table => new
                {
                    Quiz_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subject_id = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Doctor_id = table.Column<int>(type: "int", nullable: false),
                    Final_grade = table.Column<int>(type: "int", nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizTb", x => x.Quiz_id);
                    table.ForeignKey(
                        name: "FK_QuizTb_DoctorTb_Doctor_id",
                        column: x => x.Doctor_id,
                        principalTable: "DoctorTb",
                        principalColumn: "Doctor_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizTb_SubjectTb_Subject_id",
                        column: x => x.Subject_id,
                        principalTable: "SubjectTb",
                        principalColumn: "Subject_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentSubject",
                columns: table => new
                {
                    Student_id = table.Column<int>(type: "int", nullable: false),
                    Subject_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentSubject", x => new { x.Student_id, x.Subject_id });
                    table.ForeignKey(
                        name: "FK_StudentSubject_StudentTb_Student_id",
                        column: x => x.Student_id,
                        principalTable: "StudentTb",
                        principalColumn: "Student_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentSubject_SubjectTb_Subject_id",
                        column: x => x.Subject_id,
                        principalTable: "SubjectTb",
                        principalColumn: "Subject_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GradesTb",
                columns: table => new
                {
                    GradeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    AssignmentId = table.Column<int>(type: "int", nullable: true),
                    QuizId = table.Column<int>(type: "int", nullable: true),
                    GradeValue = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradesTb", x => x.GradeId);
                    table.ForeignKey(
                        name: "FK_GradesTb_AssignmentTb_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "AssignmentTb",
                        principalColumn: "Assignment_id");
                    table.ForeignKey(
                        name: "FK_GradesTb_QuizTb_QuizId",
                        column: x => x.QuizId,
                        principalTable: "QuizTb",
                        principalColumn: "Quiz_id");
                    table.ForeignKey(
                        name: "FK_GradesTb_StudentTb_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentTb",
                        principalColumn: "Student_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GradesTb_SubjectTb_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "SubjectTb",
                        principalColumn: "Subject_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Q_AttempTb",
                columns: table => new
                {
                    Q_attemp_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quiz_id = table.Column<int>(type: "int", nullable: false),
                    Student_id = table.Column<int>(type: "int", nullable: false),
                    Enrollment_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Q_AttempTb", x => x.Q_attemp_id);
                    table.ForeignKey(
                        name: "FK_Q_AttempTb_QuizTb_Quiz_id",
                        column: x => x.Quiz_id,
                        principalTable: "QuizTb",
                        principalColumn: "Quiz_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Q_AttempTb_StudentTb_Student_id",
                        column: x => x.Student_id,
                        principalTable: "StudentTb",
                        principalColumn: "Student_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Q_QuestionTb",
                columns: table => new
                {
                    Question_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quiz_id = table.Column<int>(type: "int", nullable: false),
                    Question_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Question_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Question_grade = table.Column<int>(type: "int", nullable: false),
                    Model_answer = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Q_QuestionTb", x => x.Question_id);
                    table.ForeignKey(
                        name: "FK_Q_QuestionTb_QuizTb_Quiz_id",
                        column: x => x.Quiz_id,
                        principalTable: "QuizTb",
                        principalColumn: "Quiz_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MCQOptionTb",
                columns: table => new
                {
                    Option_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question_id = table.Column<int>(type: "int", nullable: false),
                    Option_text = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MCQOptionTb", x => x.Option_id);
                    table.ForeignKey(
                        name: "FK_MCQOptionTb_Q_QuestionTb_Question_id",
                        column: x => x.Question_id,
                        principalTable: "Q_QuestionTb",
                        principalColumn: "Question_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QAttemptAnswerTb",
                columns: table => new
                {
                    Attempt_answer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Attempt_id = table.Column<int>(type: "int", nullable: false),
                    Question_id = table.Column<int>(type: "int", nullable: false),
                    Answer_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Option_id = table.Column<int>(type: "int", nullable: true),
                    Is_correct = table.Column<bool>(type: "bit", nullable: false),
                    Answer_grade = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QAttemptAnswerTb", x => x.Attempt_answer_id);
                    table.ForeignKey(
                        name: "FK_QAttemptAnswerTb_MCQOptionTb_Option_id",
                        column: x => x.Option_id,
                        principalTable: "MCQOptionTb",
                        principalColumn: "Option_id");
                    table.ForeignKey(
                        name: "FK_QAttemptAnswerTb_Q_AttempTb_Attempt_id",
                        column: x => x.Attempt_id,
                        principalTable: "Q_AttempTb",
                        principalColumn: "Q_attemp_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QAttemptAnswerTb_Q_QuestionTb_Question_id",
                        column: x => x.Question_id,
                        principalTable: "Q_QuestionTb",
                        principalColumn: "Question_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttempTb_Assignment_id",
                table: "AttempTb",
                column: "Assignment_id");

            migrationBuilder.CreateIndex(
                name: "IX_AttempTb_Student_id",
                table: "AttempTb",
                column: "Student_id");

            migrationBuilder.CreateIndex(
                name: "IX_GradesTb_AssignmentId",
                table: "GradesTb",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_GradesTb_QuizId",
                table: "GradesTb",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_GradesTb_StudentId",
                table: "GradesTb",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_GradesTb_SubjectId",
                table: "GradesTb",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_MCQOptionTb_Question_id",
                table: "MCQOptionTb",
                column: "Question_id");

            migrationBuilder.CreateIndex(
                name: "IX_Q_AttempTb_Quiz_id",
                table: "Q_AttempTb",
                column: "Quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_Q_AttempTb_Student_id",
                table: "Q_AttempTb",
                column: "Student_id");

            migrationBuilder.CreateIndex(
                name: "IX_Q_QuestionTb_Quiz_id",
                table: "Q_QuestionTb",
                column: "Quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_QAttemptAnswerTb_Attempt_id",
                table: "QAttemptAnswerTb",
                column: "Attempt_id");

            migrationBuilder.CreateIndex(
                name: "IX_QAttemptAnswerTb_Option_id",
                table: "QAttemptAnswerTb",
                column: "Option_id");

            migrationBuilder.CreateIndex(
                name: "IX_QAttemptAnswerTb_Question_id",
                table: "QAttemptAnswerTb",
                column: "Question_id");

            migrationBuilder.CreateIndex(
                name: "IX_QuizTb_Doctor_id",
                table: "QuizTb",
                column: "Doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_QuizTb_Subject_id",
                table: "QuizTb",
                column: "Subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSubject_Subject_id",
                table: "StudentSubject",
                column: "Subject_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminTb");

            migrationBuilder.DropTable(
                name: "AttempTb");

            migrationBuilder.DropTable(
                name: "GradesTb");

            migrationBuilder.DropTable(
                name: "LectureTb");

            migrationBuilder.DropTable(
                name: "QAttemptAnswerTb");

            migrationBuilder.DropTable(
                name: "StudentSubject");

            migrationBuilder.DropTable(
                name: "UniversityTb");

            migrationBuilder.DropTable(
                name: "AssignmentTb");

            migrationBuilder.DropTable(
                name: "MCQOptionTb");

            migrationBuilder.DropTable(
                name: "Q_AttempTb");

            migrationBuilder.DropTable(
                name: "Q_QuestionTb");

            migrationBuilder.DropTable(
                name: "StudentTb");

            migrationBuilder.DropTable(
                name: "QuizTb");

            migrationBuilder.DropTable(
                name: "DoctorTb");

            migrationBuilder.DropTable(
                name: "SubjectTb");
        }
    }
}
