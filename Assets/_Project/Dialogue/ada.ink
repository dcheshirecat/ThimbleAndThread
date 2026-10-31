// Ada Finch (she/her): runs Finch's Pattern Parlour, the third shop on the square. In her seventies, a retired
// pattern designer who knew your great-aunt (Aunt Win) for fifty years. Friendship only.
// The pattern shop opens once your shop has sold a few patterns (or in chapter 2). Chat with her there once a
// day; buying from her counts too. Friendship scenes at levels 1-5.
// The player's name and pronouns: {name}, {they}, {them}, {their}... (see Docs/ADDING_CONTENT.md).

=== ada_visit ===
Ada: Sit, sit. The kettle's always on in here. Patterns are better with tea. # happy
~ note(Keeps a kettle on in the pattern shop all day.)
-> END

=== ada_visit_charts ===
Ada: I drew my first chart on the back of a wallpaper sample. 1961. My mother was furious. # happy
~ note(Drew her first pattern on the back of a wallpaper sample in 1961.)
-> END

=== ada_visit_glasses ===
Ada: Have you seen my glasses? ... Ah. On my head. Again. # surprised
-> END

=== ada_visit_2_win ===
Ada: Win could never follow a pattern as written. Always "improving" it. Drove me mad. # thoughtful
Ada: She was usually right, which drove me madder.
~ note(Your great-aunt always "improved" her patterns.)
-> END

=== ada_visit_4_proud ===
Ada: Someone came in asking for "the haberdashery's patterns". Yours, {name}. Not mine. # happy
Ada: I'm only a little jealous.
-> END

=== ada_scene_1 ===
Ada lifts a tin box onto the counter. Inside: hundreds of hand-drawn charts, yellow at the edges.
Ada: Fifty years of work. Most of it never sold. # thoughtful
* [Can I look through them?]
    Ada: Gently. Some are older than you. # happy
    You find one signed "for Win, who'll change it anyway".
    ~ note(Has a tin of fifty years of hand-drawn charts.)
    ~ friendship(5)
* [Why didn't they sell?]
    Ada: Too fussy, they said. Too many colours. # thoughtful
    Ada: Then people started wanting fussy. Funny how that goes.
    ~ friendship(5)
-> END

=== ada_scene_2 ===
Ada: Your great-aunt and I had a falling-out once. Eleven years we didn't speak. # thoughtful
Ada: Over a border. A border on a tablecloth. Can you imagine.
* [Who made up first?]
    Ada: She did. Brought me a cake and the border, done her way. # happy
    Ada: It was better. I never told her.
    ~ note(Didn't speak to your great-aunt for eleven years, over a tablecloth border.)
    ~ friendship(10)
* [Eleven years is a long time.]
    Ada: It is. Don't you waste time like that. Promise me. # sad
    ~ friendship(5)
-> END

=== ada_scene_3 ===
Ada spreads a chart across the counter: a lighthouse, the harbour, little boats in half stitches.
Ada: I drew this for the town fair, the year your great-aunt opened the shop. # thoughtful
Ada: I'd like you to have it. It belongs in that shop more than in my tin.
* [Thank you, Ada. I'll treasure it.]
    Ada: Stitch it. Treasuring is for museums. # happy
    ~ friendship(10)
* [Will you show me how you drew it?]
    Ada: Pull up a stool. Mind the cat. # happy
    ~ note(Has a cat that sleeps on the pattern drawers.)
    ~ friendship(10)
-> END

=== ada_scene_4 ===
Ada: My hands aren't what they were. I can draw, but stitching hurts now. # sad
Ada: Would you stitch something for me? Just one. I've wanted to see this one finished for forty years.
* [Of course. Show me.]
    Ada: You're a good one, {name}. Win knew what she was doing, leaving it to you. # happy
    ~ friendship(10)
* [Teach me, and we'll do it together.]
    Ada: Together. Yes. I'd like that very much. # happy
    ~ friendship(10)
-> END

=== ada_scene_5 ===
Ada presses a small folded page into your hand. Old paper, Win's handwriting on one side, Ada's on the other.
Ada: She gave it to me for safekeeping, a long time ago. I think it's done its waiting. # thoughtful
~ flag(ada_page)
~ friendship(15)
-> END

// ---- At the shop door: a hello, a goodbye (ada_thanks* after buying). Add variants freely.

=== ada_greet ===
Ada: Come in, come in. Shut the door, the patterns blow about. # happy
-> END

=== ada_greet_new ===
Ada: New ones in the drawer by the window. Tell me what you think. Honestly. # thoughtful
-> END

=== ada_greet_2_friend ===
Ada: There {they} {is}. I saved you the good chair. # happy
-> END

=== ada_bye ===
Ada: Nothing today? Patterns are patient. They'll wait. # happy
-> END

=== ada_bye_2_tea ===
Ada: Off you go. Next time you're staying for a whole cup. # happy
-> END

=== ada_thanks ===
Ada: A good choice. That one practically stitches itself. Practically. # happy
-> END

=== ada_thanks_2_fussy ===
Ada: Oh, you picked a fussy one. Good. Fussy sells. # happy
-> END

// ---- First meeting: she introduces herself. ~ met() puts her name and portrait in the Journal.

=== ada_intro ===
A small woman in a cardigan looks up from a drawing board, a pencil behind each ear. # surprised
Ada: Win's {niece}. I'd know that face anywhere. Come here, let me look at you. # happy
Ada: Ada Finch. I draw patterns. Drew them with your great-aunt for fifty years, and argued about every one. # happy
~ met()
~ note(Runs the pattern shop. Drew patterns with your great-aunt for fifty years.)
Ada: Your shop sells patterns now, I hear. Then you'll be needing better ones than Win's book. # thoughtful
-> END

// ---- Saturdays (owner, 2026-10-31): the shop is closed; friends at level 2+ can be visited (name_saturday*),
// and someone you're seeing takes you on a date (name_date*). Add variants freely.

=== ada_saturday ===
Ada makes you sit with her cat while she draws, and talks about the town fifty years ago. # happy
Ada: The shop was a fish shop before Win took it. She scrubbed it for a month. Still smelled of mackerel at Christmas. # happy
-> END
