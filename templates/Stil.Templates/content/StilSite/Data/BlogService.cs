namespace StilSite.Data;

/// <summary>A blog post: its address (Slug), the card's texts and the article's sections.</summary>
public sealed record BlogPost(
    string Slug,
    string Title,
    string Excerpt,
    string Author,
    DateTime Date,
    string ReadingTime,
    string[] Tags,
    string? Image,
    string Intro,
    PostSection[] Sections);

/// <summary>A part of a post under its own heading. Id is the heading's anchor, for the table of contents.</summary>
public sealed record PostSection(string Id, string Heading, string[] Paragraphs);

/// <summary>The blog's posts, newest first. Replace SampleBlogService with your own source.</summary>
public interface IBlogService
{
    IReadOnlyList<BlogPost> All();

    BlogPost? Find(string slug);
}

/// <summary>Sample posts, kept in memory.</summary>
public sealed class SampleBlogService : IBlogService
{
    private static readonly BlogPost[] Posts =
    [
        new("calm-mondays", "Five rules for a calm Monday",
            "The week is decided on Friday afternoon. Five habits from teams that start Monday with a plan.",
            "Maja Lind", new DateTime(2026, 11, 3), "5 min read", ["Planning", "Teams"], "images/post-monday.svg",
            "Every team we talk to has the same Monday: the first hour goes to working out who does what.",
            [
                new("why", "Why Mondays go wrong",
                    ["A plan made on Monday morning is made under pressure, with half the facts."]),
                new("friday", "Plan on Friday",
                    ["On Friday afternoon the week behind you is fresh, and the week ahead is still calm. Twenty minutes then saves an hour on Monday."]),
                new("rules", "The five rules",
                    ["Book the shared things first. Put time off in the plan. Leave a fifth of each day empty. Give each task one owner. Share the plan where people look."]),
            ]),
        new("book-the-van", "Book the van, not the argument",
            "Shared resources cause more friction than shared tasks. How to book them so nobody has to ask.",
            "Erik Holm", new DateTime(2026, 10, 27), "4 min read", ["Rooms"], "images/post-rooms.svg",
            "The meeting room, the van and the good projector: whoever books first wins, and everybody else asks around.",
            [
                new("visible", "Make bookings visible",
                    ["When everybody sees the bookings in the same calendar as their tasks, the questions stop."]),
                new("rules", "Agree on two rules",
                    ["Book it when you plan it, and free it when the plan changes. That is all most teams need."]),
            ]),
        new("your-data", "Your data leaves with you",
            "Why every plan exports to CSV, and what we do with the data we keep.",
            "Jonas Ek", new DateTime(2026, 10, 20), "3 min read", ["Data"], "images/post-export.svg",
            "A tool you cannot leave is a tool you cannot trust.",
            [
                new("export", "Export everything",
                    ["Every plan, booking and comment exports to CSV, at any time, on every plan, the free one too."]),
            ]),
        new("time-off", "Time off, now in the planner",
            "Holidays and sick days show up in the week, so nobody plans around a person who is away.",
            "Anna Niemi", new DateTime(2026, 10, 13), "2 min read", ["News"], null,
            "A person who is away is not a free slot. Now the planner knows that too.",
            [
                new("how", "How it works",
                    ["Add time off from the calendar or import it from your HR system. It shows on the person's row and in the week."]),
            ]),
    ];

    public IReadOnlyList<BlogPost> All() => Posts;

    public BlogPost? Find(string slug) => Posts.FirstOrDefault(post => post.Slug == slug);
}
