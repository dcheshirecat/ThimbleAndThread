// Bram Okafor (he/him): runs the Supply Market. Adult; romanceable. There from chapter 1.
// A big warm laugh, knows every thread count by heart, gives out lemon sweets to everyone.
// Chat with him at the market (once a day); buying from him counts too. Friendship scenes at levels 1-3;
// at level 3 the player can ask him out (~ romance(1)); scenes 4 and 5 follow the romance if they did.
// The player's name and pronouns: {name}, {they}, {them}, {their}... (see Docs/ADDING_CONTENT.md).

=== bram_visit ===
Bram: {name}! My favourite customer. Don't tell the others. Actually, tell them, it's good for business. # happy
-> END

=== bram_visit_delivery ===
Bram: New delivery came in this morning. I've been stroking the merino like a cat. # happy
Bram: That's normal. That's a normal thing for a grown man to do.
-> END

=== bram_visit_sweets ===
Bram: Lemon sweet? I keep a tin for the kids. And for me. Mostly for me. # happy
-> END

=== bram_visit_2_aunt ===
Bram: Your great-aunt used to come in every Tuesday. Same list, every week, written on the back of a receipt. # thoughtful
Bram: I kept one. Silly, really. I'll show you sometime.
-> END

=== bram_visit_4_saved ===
Bram: I put the good skeins aside for you before I opened. Don't tell anyone. # happy
Bram: Especially not Rowan. Rowan has a nose for good skeins.
-> END

=== bram_scene_1 ===
Bram: Got a minute? # thoughtful
He pulls a creased receipt out of his till drawer. On the back, in neat old-fashioned writing: "Tuesday: navy, red, white, a little gold. Ask after his mother."
Bram: Your great-aunt. Every week she asked after my mum. Every single week.
* [She sounds like she cared about you.]
    Bram: She did. Whole town, really. And now you're doing it. You know that? # happy
    ~ friendship(5)
* [How is your mum?]
    Bram: ... You're the first person to ask in two years. # surprised
    Bram: She's good. She'd like you. Everybody's mum would like you.
    ~ friendship(10)
-> END

=== bram_scene_2 ===
After closing, the market is all stacked crates and a radio playing something old. Bram is sorting buttons by colour.
Bram: Help me? It's soothing. Forty years and I still find it soothing. # happy
* [Sort the blues]
    You sort the blues. He hums along to the radio, badly and happily.
    ~ friendship(5)
* [Ask why he never got married]
    Bram: Ha! Straight in. I like that. # surprised
    Bram: Never met anyone who'd put up with the buttons. # thoughtful
    ~ friendship(5)
-> END

=== bram_scene_3 ===
Bram: Harbour lights festival next week. I always do the hot chocolate stall. # thoughtful
Bram: It's a two-person stall, really. I just never had the second person.
* [I'll be your second person.]
    Bram: Yeah? As in... yeah? # surprised
    Bram: Right. Good. Brilliant. I'm going to go and stand in the stockroom for a minute. # happy
    ~ romance(1)
    ~ friendship(10)
* [I'll come and buy one. Two, even.]
    Bram: Ha! I'll put extra marshmallows in yours. # happy
    ~ friendship(5)
-> END

=== bram_scene_4 ===
* {romance} [The hot chocolate stall]
    The two of you run out of marshmallows by seven. Bram doesn't seem to mind at all.
    Bram: Best festival I've ever done. And I've done twenty-two. # happy
    ~ friendship(10)
* {not romance} [Help him restock the shelves]
    Bram: You're a star, {name}. A genuine star. # happy
    ~ friendship(5)
-> END

=== bram_scene_5 ===
* {romance} [Dinner above the market]
    His flat above the market smells of cardamom and wool. There's a jar of buttons on every windowsill.
    Bram: I sorted them by colour for you. Every jar. # thoughtful
    Bram: I'm not good at saying things. So. That's what it means. # happy
    ~ flag(bram_together)
    ~ friendship(15)
* {not romance} [Sort the new delivery together]
    Bram: Same time next week? It's a date. A non-date. A button date. # happy
    ~ friendship(10)
-> END
