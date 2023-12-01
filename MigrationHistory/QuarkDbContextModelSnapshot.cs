﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Quark_Backend.DAL;

#nullable disable

namespace Quark_Backend.MigrationHistory
{
    [DbContext(typeof(QuarkDbContext))]
    partial class QuarkDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Quark_Backend.Entities.Conversation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.HasKey("Id")
                        .HasName("conversations_pkey");

                    b.ToTable("conversations", (string)null);
                });

            modelBuilder.Entity("Quark_Backend.Entities.Department", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("departments_pkey");

                    b.ToTable("departments", (string)null);
                });

            modelBuilder.Entity("Quark_Backend.Entities.JobPosition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<int>("DepartmentId")
                        .HasColumnType("integer")
                        .HasColumnName("department_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("job_positions_pkey");

                    b.HasIndex("DepartmentId");

                    b.ToTable("job_positions", (string)null);
                });

            modelBuilder.Entity("Quark_Backend.Entities.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<int>("ConversationId")
                        .HasColumnType("integer")
                        .HasColumnName("conversation_id");

                    b.Property<DateOnly>("SentDate")
                        .HasColumnType("date")
                        .HasColumnName("sent_date");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("text");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("messages_pkey");

                    b.HasIndex("ConversationId");

                    b.HasIndex("UserId");

                    b.ToTable("messages", (string)null);
                });

            modelBuilder.Entity("Quark_Backend.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("email");

                    b.Property<string>("FirstName")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("first_name");

                    b.Property<int?>("JobId")
                        .HasColumnType("integer")
                        .HasColumnName("job_id");

                    b.Property<string>("LastName")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("last_name");

                    b.Property<string>("Password")
                        .HasMaxLength(40)
                        .HasColumnType("character varying(40)")
                        .HasColumnName("password");

                    b.Property<int?>("PermissionLevel")
                        .HasColumnType("integer")
                        .HasColumnName("permission_level");

                    b.Property<string>("PictureUrl")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("picture_url");

                    b.Property<string>("SelfDescription")
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)")
                        .HasColumnName("self_description");

                    b.Property<string>("Username")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)")
                        .HasColumnName("username");

                    b.HasKey("Id")
                        .HasName("users_pkey");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("JobId");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("Quark_Backend.Entities.UsersConversation", b =>
                {
                    b.Property<int>("UsersId")
                        .HasColumnType("integer")
                        .HasColumnName("users_id");

                    b.Property<int>("ConversationsId")
                        .HasColumnType("integer")
                        .HasColumnName("conversations_id");

                    b.HasKey("UsersId", "ConversationsId");

                    b.HasIndex("ConversationsId");

                    b.ToTable("users_conversations", (string)null);
                });

            modelBuilder.Entity("Quark_Backend.Entities.JobPosition", b =>
                {
                    b.HasOne("Quark_Backend.Entities.Department", "Department")
                        .WithMany("JobPositions")
                        .HasForeignKey("DepartmentId")
                        .IsRequired()
                        .HasConstraintName("job_positions_department_id_fkey");

                    b.Navigation("Department");
                });

            modelBuilder.Entity("Quark_Backend.Entities.Message", b =>
                {
                    b.HasOne("Quark_Backend.Entities.Conversation", "Conversation")
                        .WithMany("Messages")
                        .HasForeignKey("ConversationId")
                        .IsRequired()
                        .HasConstraintName("messages_conversation_id_fkey");

                    b.HasOne("Quark_Backend.Entities.User", "User")
                        .WithMany("Messages")
                        .HasForeignKey("UserId")
                        .IsRequired()
                        .HasConstraintName("messages_user_id_fkey");

                    b.Navigation("Conversation");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Quark_Backend.Entities.User", b =>
                {
                    b.HasOne("Quark_Backend.Entities.JobPosition", "JobPosition")
                        .WithMany("Users")
                        .HasForeignKey("JobId")
                        .HasConstraintName("users_job_positions_id_fkey");

                    b.Navigation("JobPosition");
                });

            modelBuilder.Entity("Quark_Backend.Entities.UsersConversation", b =>
                {
                    b.HasOne("Quark_Backend.Entities.Conversation", "Conversations")
                        .WithMany()
                        .HasForeignKey("ConversationsId")
                        .IsRequired()
                        .HasConstraintName("users_conversations_conversations_id_fkey");

                    b.HasOne("Quark_Backend.Entities.User", "Users")
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .IsRequired()
                        .HasConstraintName("users_conversations_users_id_fkey");

                    b.Navigation("Conversations");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("Quark_Backend.Entities.Conversation", b =>
                {
                    b.Navigation("Messages");
                });

            modelBuilder.Entity("Quark_Backend.Entities.Department", b =>
                {
                    b.Navigation("JobPositions");
                });

            modelBuilder.Entity("Quark_Backend.Entities.JobPosition", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("Quark_Backend.Entities.User", b =>
                {
                    b.Navigation("Messages");
                });
#pragma warning restore 612, 618
        }
    }
}
