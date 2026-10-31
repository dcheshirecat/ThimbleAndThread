// Theo Lindqvist (he/him): Tobias's grandson, nineteen, back from the city to learn the boats. Shy, funny once he warms up.
// Friendship only. Chapter 2. Knot names follow Docs/ADDING_CONTENT.md, "Dialogue".

=== theo_intro ===
A lanky young man in a fisherman's jumper three sizes too big ducks under the door frame. # thoughtful
Theo: Grandad said to come in. Tobias. He's my grandad. I'm Theo. # happy
~ met()
~ note(Tobias's grandson, back from the city to learn the boats.)
Theo: He talks about this shop more than he talks about fish. That's a lot.
-> END

=== theo_visit ===
Theo: Gutted twelve mackerel before breakfast. I'm a changed man. # happy
-> END

=== theo_visit_jumper ===
Theo: This was Gran's jumper. Grandad gave it me. It's enormous. I'm keeping it forever. # happy
~ note(Wears his late gran's jumper.)
-> END

=== theo_visit_2_city ===
Theo: Everyone in the city asked what I was going to be. Nobody here asks. They just hand you a rope. # thoughtful
-> END

=== theo_request_grandad ===
# fits: scarf, sweater, tablecloth
# best: scarf
Theo: Grandad's birthday's coming. He says he doesn't want anything. # thoughtful
Theo: He's lying. He wants something Gran would've made. I can't make it. Can you?
-> END

=== theo_request_boat ===
# fits: tote, hoop_art, sampler
# best: tote
Theo: I keep my lunch in a bucket on the boat. It always tastes of bait. # sad
Theo: Something to keep things dry. And not bait-flavoured.
-> END

=== theo_handover_good ===
Theo: Grandad's going to lose it. In a quiet way. He does everything in a quiet way. # happy
-> END

=== theo_handover_okay ===
Theo: Ace. Thanks. Really. # happy
-> END

=== theo_handover_poor ===
Theo: It's handmade. Grandad says handmade beats perfect. # thoughtful
-> END

=== theo_suggest_yes ===
Theo: Yeah, go on. Make me one. # happy
-> END

=== theo_suggest_buy ===
Theo: Could I have the pattern? Grandad says Gran had hands like yours. I want to try. # happy
-> END

=== theo_suggest_no ===
Theo: Bit fancy for the boat, maybe. # thoughtful
-> END

=== theo_scene_1 ===
Theo: Can I tell you something? I dropped out. Of uni. Grandad doesn't know. # sad
* [You should tell him.]
    Theo: I know. I will. Will you... be around, after? # thoughtful
    ~ friendship(10)
* [He'll be glad you're home.]
    Theo: You think? ... Yeah. Maybe. # happy
    ~ friendship(10)
-> END

=== theo_scene_2 ===
Theo: I told him. He didn't say anything for ages. Then he gave me the keys to the Elsa May. # surprised
Theo: He's never let anyone drive her. Not even Dad.
* [That means the world.]
    Theo: I know. I cried. Don't tell anyone. # happy
    ~ friendship(10)
* [Take me out on her sometime.]
    Theo: First trip. Promise. # happy
    ~ friendship(10)
-> END

=== theo_scene_3 ===
Theo: I want to learn to knit. Like Gran. Will you teach me? Properly? # thoughtful
His first scarf is wonky, full of holes, and he wears it every single day.
~ friendship(15)
-> END

=== theo_saturday ===
Theo takes you out on the Elsa May. Tobias waves from the harbour wall the whole way out, and the whole way back. # happy
Theo: He's pretending he isn't proud. He's terrible at pretending. # happy
-> END
