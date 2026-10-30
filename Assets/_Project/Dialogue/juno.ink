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
* [Enter. I'll help you finish the jacket.]
    ~ friendship(10)
    ~ flag(juno_contest)
    Juno: Okay. Okay okay okay. We're doing this. # happy
* [Next year, when you're ready.]
    Juno: Yeah. Maybe. # sad
- Juno: Thanks for not laughing, anyway.
-> END
