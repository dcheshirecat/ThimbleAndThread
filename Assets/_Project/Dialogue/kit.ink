// Kit Calloway (they/them): a painter who moved here for the light, thirties, paint on everything, wildly enthusiastic. Friendship only. Chapter 4.
// Knot names follow Docs/ADDING_CONTENT.md, "Dialogue".

=== kit_intro ===
Someone with paint up to their elbows bursts in, pointing at your window display. # surprised
Kit: Those colours! Who chose those colours? Was it you? I'm Kit. I paint. Badly, sometimes. Mostly not. # happy
~ met()
~ note(A painter who moved here for the light.)
-> END

=== kit_visit ===
Kit: The light today is outrageous. I've been out since five. # happy
-> END

=== kit_visit_paint ===
Kit: I've got paint in my ear. I don't know how. I wasn't painting near my ear. # surprised
-> END

=== kit_visit_2_show ===
Kit: I'm having a show at the lifeboat house. You're coming. That's not a question. # happy
~ note(Having an art show at the lifeboat house.)
-> END

=== kit_request_cold ===
# fits: sweater, scarf
# best: sweater
Kit: My studio's an old net loft. It's freezing. My fingers go blue. # sad
Kit: Something warm that's also a colour. I hate beige. Beige is a crime.
-> END

=== kit_request_muse ===
# fits: plushie, plushie_sewn, hoop_art
# best: plushie
Kit: I need a muse. Something small and odd to sit on my easel. # thoughtful
Kit: Make it strange. I trust you.
-> END

=== kit_handover_good ===
Kit: Oh, that's a masterpiece. I'm painting it. I'm painting you painting it. # happy
-> END

=== kit_handover_okay ===
Kit: Love it. Love the colours. # happy
-> END

=== kit_handover_poor ===
Kit: It's honest. Honest is good. Honest is underrated. # thoughtful
-> END

=== kit_suggest_yes ===
Kit: YES. Colour me in. # happy
-> END

=== kit_suggest_buy ===
Kit: I'll make one myself. Can I have the chart? # happy
-> END

=== kit_suggest_no ===
Kit: Not my palette, sorry! # thoughtful
-> END

=== kit_scene_1 ===
Kit: Can I paint your shop? From outside. In the morning light. # happy
* [I'd be honoured.]
    Kit: Don't move anything. Not a thread. # happy
    ~ friendship(10)
* [Only if I can stitch a copy.]
    Kit: A stitched painting of a painting of a stitch shop. Yes. Absolutely yes. # happy
    ~ friendship(10)
-> END

=== kit_scene_2 ===
Kit: Nobody came to my last show. In the city. Not one person. # sad
Kit: That's why I came here, I think.
* [Everyone here will come.]
    Kit: You can't promise that. ... Can you? # thoughtful
    ~ friendship(10)
* [I'll be the first one through the door.]
    Kit: Okay. Okay. That's one. That's enough. # happy
    ~ friendship(15)
-> END

=== kit_scene_3 ===
Kit: The show sold out. Every painting. Your shop's one went first. # surprised
Kit: I kept the sketch for you.
~ friendship(15)
-> END

=== kit_saturday ===
Kit sets up two easels on the cliff and makes you paint the sea. Yours looks like soup. They frame it anyway. # happy
Kit: That's the most honest sea I've ever seen. # happy
-> END

// ---- More lines (2026-11-02: so nobody repeats themselves), and how they take a present ----
=== kit_visit_blue ===
Kit: I've found a new blue! It's the sea at half past four on a Tuesday. Nobody believes me. # surprised
-> END
=== kit_visit_mess ===
Kit: Is that paint on my ear? It's always paint on my ear. I've given up. # happy
-> END
=== kit_visit_3_doubt ===
Kit: Some days I think every painting I've done is rubbish. # sad
Kit: Then I come in here and look at all the colours on your shelves and I want to try again. So, thank you, I suppose.
-> END
=== kit_saturday_sketch ===
You sit with Kit on the harbour steps and they teach you to draw boats with three lines each. Yours look like teapots. # happy
Kit: Teapots that float! That's a whole series! # happy
-> END
=== kit_gift ===
Kit: Oh, the stitches! Look at the stitches! I'm going to paint it. Can I paint it? I'm painting it. # surprised
-> END
