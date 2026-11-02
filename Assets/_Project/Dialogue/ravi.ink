// Ravi Sen (they/them): runs Byte & Bobbin, a repair shop for anything with a plug or a pedal, in the old chandlery
// loft. Adult; romanceable. Opens once you reach level 9. Fixes sewing machines and builds clever gadgets.
// The player's name and pronouns: {name}, {they}...

=== ravi_intro ===
Someone in a soldering mask waves a screwdriver at you through a cloud of smoke. # surprised
Ravi: Sorry! Sorry. That was meant to happen. Mostly. # happy
Ravi: Ravi Sen. I fix things, and sometimes I make things that fix other things. Byte & Bobbin.
~ met()
~ note(Runs Byte & Bobbin: repairs, gadgets, automations.)
Ravi: If you ever want the shop to run itself a bit, I'm your person. # thoughtful
-> END

=== ravi_visit ===
Ravi: I fixed your great-aunt's sewing machine once. It still had a bus ticket from 1964 in the bobbin case. # happy
-> END

=== ravi_visit_robot ===
Ravi: Meet Bobbin. Bobbin sorts screws. Bobbin is not good at sorting screws. # happy
~ note(Has a screw-sorting robot called Bobbin.)
-> END

=== ravi_scene_1 ===
Ravi: Can I show you something? I built a little machine that winds bobbins by itself. # thoughtful
* [That's brilliant!]
    Ravi: You think so? Nobody ever says that. They say "is it on fire". # happy
    ~ friendship(5)
* [Could it wind thread for me?]
    Ravi: For you? It'll wind anything you like. # happy
    ~ friendship(10)
-> END

=== ravi_scene_2 ===
Ravi: I left the city because everything there was about being fast. Here things can be slow. And mended. # thoughtful
~ note(Left the city to live somewhere slower, where things get mended.)
~ friendship(5)
-> END

=== ravi_scene_3 ===
Ravi: I've been working on something for you. It's a music box. It plays the doorbell of your shop, which, it turns out, is in B flat. # thoughtful
* [Ravi... would you like to have dinner with me?]
    Ravi: I... yes. Yes! Hold on, I need to turn off the soldering iron. # happy
    ~ romance(1)
* [It's perfect. Thank you, friend.]
    Ravi: Friend. Good. Friends are the best thing I've ever built. # happy
    ~ friendship(5)
-> END

=== ravi_greet ===
Ravi: Hi! Don't touch the blue wire. Or the red one. Actually, just don't touch. # happy
-> END

=== ravi_greet_2_friend ===
Ravi: {name}! Perfect timing, I need someone to say "wow" at a thing. # happy
-> END

=== ravi_bye ===
Ravi: Come back soon! Bobbin will miss you. Bobbin can't miss things, but still. # happy
-> END

=== ravi_thanks ===
Ravi: Installed and tested. Twice. Okay, once. It'll be fine! # happy
-> END

=== ravi_saturday ===
Ravi takes you to a car-boot sale and buys a broken radio "for parts", then fixes it by lunchtime. # happy
-> END

=== ravi_date ===
Ravi has built a tiny planetarium out of a colander and a torch. You lie on their workshop floor and watch the holes shine. # happy
-> END

// ---- More lines (2026-11-02: so nobody repeats themselves), and how they take a present ----
=== ravi_visit_fix ===
Ravi: Fixed a toaster, two phones and a hearing aid before lunch. The hearing aid belonged to the old postman. He says he liked the quiet. # happy
-> END
=== ravi_visit_bobbin ===
Ravi: Bobbin sorted every screw in the loft by size, then by colour, then by how happy they looked. I didn't program the last one. # surprised
-> END
=== ravi_visit_3_city ===
Ravi: In the city nobody mended anything. You just bought another. # thoughtful
Ravi: Here people bring me things their parents owned. It feels like I'm keeping something going.
-> END
=== ravi_saturday_radio ===
Ravi lets you help rebuild an old valve radio. When it crackles on, it's playing a shipping forecast. Ravi looks delighted. # happy
Ravi: Perfect. Exactly what I hoped for. # happy
-> END
=== ravi_gift ===
Ravi: Handmade? No circuit boards at all? I love it. I'm going to put it where Bobbin can admire it. # happy
-> END
