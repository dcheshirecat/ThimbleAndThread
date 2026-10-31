// Juno: a teenager who wants knitwear that doesn't look grandma-made. Friendship only (Juno is a minor:
// never romanceable). Teaches French knots at friendship level 1.

=== juno_visit ===
Juno: Hey. Is it weird that this shop smells like a library? It's weird. I like it. # happy
-> END

=== juno_visit_2 ===
Juno: Okay so I wore the thing you made and THREE people asked where I got it. # happy
Juno: I said "a secret shop". Very mysterious. You're welcome.
-> END

=== juno_request_skate ===
# fits: scarf, sweater, tote
# best: scarf
Juno: The skate park is basically a wind tunnel with ramps. # thoughtful
~ note(Skateboards.)
Juno: I need something round my neck. It has to look cool though. If it looks like a present from an aunt I'm leaving it on the bus.
-> END

=== juno_request_bag ===
# fits: tote, hoop_art, sampler
# best: tote
Juno: My bag split. Again. Everything fell out in front of literally everyone. # sad
Juno: I need something that carries a sketchbook and doesn't look like it came free with a magazine.
-> END

=== juno_handover_good ===
Juno: Oh that's SICK. Sorry, I mean, that's very nice. No, it's sick. # happy
-> END

=== juno_handover_okay ===
Juno: Nice! It's got character. That's a good thing. Mostly. # happy
-> END

=== juno_handover_poor ===
Juno: It's... handmade. You can totally tell it's handmade. That's like, the point. Right? # thoughtful
-> END

=== juno_scene_1 ===
Juno: Can I show you something? # thoughtful
She pulls out a sketchbook. Every page is clothes: jackets covered in tiny embroidered stars, a scarf like a comet.
Juno: My mum says it's a phase. I say it's a career.
~ note(Fills sketchbooks with clothes. Wants to be a designer.)
* [These are brilliant.]
    Juno: You actually think so? # surprised
    ~ friendship(5)
* [Want to learn how to stitch the stars?]
    Juno: Wait, you'd teach me? # surprised
    ~ friendship(5)
- Juno: Okay. Look. The stars are French knots. You wrap the thread round twice and go back down right next to where you came up. Like this.
Juno: Ha. I'm teaching YOU now. Character development. # happy
-> END

=== juno_scene_2 ===
Juno: There's a fashion thing at the college. Students can enter. # thoughtful
Juno: I'm not a student. I'm sixteen. But I could... maybe... enter anyway?
~ note(Is sixteen.)
* [Enter. I'll help you finish the jacket.]
    ~ friendship(10)
    ~ flag(juno_contest)
    Juno: Okay. Okay okay okay. We're doing this. # happy
* [Next year, when you're ready.]
    Juno: Yeah. Maybe. # sad
- Juno: Thanks for not laughing, anyway.
-> END

// ---- More lines (2026-10-30). Juno is a teen: friendship only, never romanceable.

=== juno_visit_skate ===
Juno: Landed a kickflip today. Nobody saw. That still counts, right? # happy
-> END

=== juno_visit_homework ===
Juno: I'm meant to be at the library. If my mum asks, this is a library. # neutral
Juno: There are patterns. Patterns are reading.
-> END

=== juno_visit_1_band ===
Juno: My friend's starting a band. I'm doing the merch. # happy
~ note(Does the merch for a friend's band.)
Juno: By merch I mean I want to sew patches on everything we own.
-> END

=== juno_visit_2_nan ===
Juno: Okay, don't tell anyone, but I showed my nan the stuff from here. # thoughtful
Juno: She said it was "very modern". Which is either a compliment or a threat.
-> END

=== juno_visit_3_teach ===
Juno: Can you show me how you do that? The thing with your hands. Like, the fast thing. # surprised
Juno: Not for me. For a friend. The friend is me.
-> END

=== juno_request_gig ===
# fits: tote, hoop_art, sampler
# best: tote
Juno: Gig on Saturday. I need to carry, like, cables and snacks and my whole personality. # neutral
Juno: Something that doesn't look like it came free with a magazine.
-> END

=== juno_handover_good_wow ===
Juno: No way. No WAY. This is actually cool. # surprised
Juno: I'm going to pretend I made it. Kidding. Mostly. # happy
-> END

=== juno_handover_okay_cool ===
Juno: Cool. Yeah. It's cool. I'll wear it ironically. And then also normally. # neutral
-> END

=== juno_handover_poor_honest ===
Juno: It's... a choice? Respect the choice. # thoughtful
-> END

// ---- Reactions when you suggest a pattern on display: _yes (they want you to make it), _buy (they make
// that craft themselves), _no. Add variants like name_suggest_no_rain. ----

=== juno_suggest_yes ===
Juno: Okay, that's actually cool. I'd wear that. Unironically. # happy
-> END

=== juno_suggest_buy ===
Juno: Wait, I could make that. Can I make that? I'm making that. # surprised
-> END

=== juno_suggest_no ===
Juno: It's nice. It's very... nice. That's the problem. # neutral
-> END

// ---- First meeting (owner, 2026-10-31): they introduce themselves. ~ met() puts their name and portrait
// in the Journal (until then they show as ???). Played once, instead of the usual hello.

=== juno_intro ===
A teenager with headphones round their neck and paint on their jeans pushes the door with one finger. # neutral
Juno: Hey. I'm Juno. I walk past here every day on the way to the skate park. # happy
~ met()
Juno: It's been shut forever. I kind of assumed it was haunted.
~ note(Goes to the skate park most days.)
Juno: Is it haunted? It's fine if it's haunted. # happy
-> END
