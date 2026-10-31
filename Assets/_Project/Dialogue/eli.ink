// Eli Brooks (he/him): runs the Driftwood, the café on the harbour. Adult; romanceable. Arrives in chapter 2.
// Warm, a little scattered, terrible at keeping his own hands warm. Friendship scenes at levels 1-3; at
// level 3 the player can ask him out (~ romance(1)); scenes 4 and 5 follow the romance if they did.

=== eli_visit ===
Eli: Hi! Eli. The Driftwood, down on the harbour? The café with the wonky sign. # happy
~ note(Runs the Driftwood, the café on the harbour.)
Eli: I've been meaning to come in for weeks. Your shop smells better than my coffee. That's a worry.
-> END

=== eli_visit_cold ===
Eli: My hands are freezing. Coffee machine's broken again. Don't touch them, they're like fish. # thoughtful
-> END

=== eli_visit_cake ===
Eli: Brought you a slice of the lemon cake. It didn't sell. That's not why I brought it. # happy
Eli: Okay, it's partly why I brought it.
-> END

=== eli_visit_1_regulars ===
Eli: Tobias has the same table every morning. Same tea. Same crossword. # thoughtful
Eli: He asked about you yesterday. That's practically a speech, from him.
-> END

=== eli_visit_2_music ===
Eli: Open mic night on Friday. I'm playing. Badly. On purpose, so nobody else is nervous. # happy
~ note(Plays at the café's open mic nights.)
-> END

=== eli_visit_4_morning ===
Eli: Saved you the window table. Nobody's allowed to sit there before nine now. # happy
Eli: New rule. Very strict. I made it up this morning.
-> END

=== eli_request_aprons ===
# fits: tote, tablecloth, sampler
# best: tablecloth
Eli: The café tables are bare wood with forty years of coffee rings. # thoughtful
Eli: I want it to feel like somebody's kitchen. Somebody nice's kitchen.
-> END

=== eli_request_hands ===
# fits: scarf, sweater
# best: scarf
Eli: I stand by the harbour door all winter. It opens every two minutes and the wind comes in sideways. # sad
Eli: Something warm. I'd wear it every single day, I promise.
-> END

=== eli_request_wall ===
# fits: hoop_art, sampler
# best: hoop_art
Eli: There's a nail on the café wall with nothing on it. It's been there since I bought the place. # thoughtful
Eli: It deserves something. Something with the sea in it.
-> END

=== eli_handover_good ===
Eli: Oh, wow. Wow. I'm putting this right where everyone can see it. # happy
Eli: And when they ask, I'm sending them to you.
-> END

=== eli_handover_good_hands ===
Eli: Look, I can feel my fingers! Both hands! # happy
-> END

=== eli_handover_okay ===
Eli: It's lovely. Honestly. It'll be loved to bits, and that's the best thing that can happen to anything. # happy
-> END

=== eli_handover_poor ===
Eli: It's got character! Like the café. Like me. # thoughtful
-> END

=== eli_scene_1 ===
Eli: Can I ask a weird question? # thoughtful
Eli: How do you make things that last? Everything I make gets eaten in ten minutes.
* [That's the point of cake.]
    Eli: Huh. I've never thought of it like that. # happy
    ~ friendship(5)
* [Come by after closing. I'll show you a stitch.]
    Eli: Really? I'll bring the leftovers. There are always leftovers. # surprised
    ~ friendship(5)
-> END

=== eli_scene_2 ===
After closing, the Driftwood is all fairy lights and upturned chairs. Eli makes two hot chocolates with far too much cream.
Eli: My gran ran this place. She used to say the harbour was the town's front door. # thoughtful
~ note(The café was his gran's.)
Eli: I just want people to feel like somebody's glad they came in.
* [They do. I do.]
    Eli: ... Thanks. That means more than you'd think. # happy
    ~ friendship(10)
* [Your gran sounds wonderful.]
    Eli: She was. She'd have liked you. She'd have fed you until you couldn't move. # happy
    ~ friendship(5)
-> END

=== eli_scene_3 ===
Eli: So the open mic went... fine. Nobody threw anything. # happy
Eli: I wrote a song, actually. Not about anyone. It's about the harbour. Mostly.
~ note(Writes songs. One is about the harbour. Mostly.)
* [Will you play it for me?]
    Eli: Right now? Here? Okay. Okay. Don't look at me while I play it. # surprised
    He plays. It's about the harbour. It's also, quite clearly, about a shop with its light still on.
    Eli: So that's... that's the song. Would you, um. Would you like to get dinner sometime? # thoughtful
    ~ romance(1)
    ~ friendship(10)
* [You should play it on Friday.]
    Eli: Maybe I will. You'll come, right? Front row. # happy
    ~ friendship(5)
-> END

=== eli_scene_4 ===
* {romance} [Dinner at the Driftwood]
    He's closed the café for the night. One table, two candles, and a very nervous lasagne.
    Eli: I've never cooked for anyone before. Customers don't count. # thoughtful
    Eli: Is it okay? You can be honest. Please don't be honest. # happy
    ~ friendship(10)
* {not romance} [Help with the morning rush]
    Eli: You're a lifesaver. Next coffee's on me. All of them, forever. # happy
    ~ friendship(5)
-> END

=== eli_scene_5 ===
* {romance} [Walk out to the end of the pier]
    The tide is out and the whole bay is silver. Eli has his hands in the pockets of the scarf you made him.
    Eli: My gran said you know a place is home when someone's glad you came in. # thoughtful
    Eli: I'm always glad when you come in. I wanted you to know that. # happy
    ~ flag(eli_together)
    ~ friendship(15)
* {not romance} [Paint the café sign together]
    The sign is still a bit wonky. Eli says it's character.
    Eli: Thanks for being here. For all of it. # happy
    ~ friendship(10)
-> END
