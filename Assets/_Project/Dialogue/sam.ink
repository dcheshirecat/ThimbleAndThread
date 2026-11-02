// Sam Marsh-to-be (they/them): Benji's fiancé, a primary-school music teacher. Calm, organised, quietly very funny.
// Friendship only. Chapter 3. Knot names follow Docs/ADDING_CONTENT.md, "Dialogue".

=== sam_intro ===
Someone with a ukulele case on their back and a wedding planner under their arm comes in. # happy
Sam: Hi! I'm Sam. Benji's other half. The calm half. # happy
~ met()
~ note(Benji's fiancé. Teaches music at the primary school.)
Sam: He's told me about your shop. He's told me about everything. Benji tells me about everything.
-> END

=== sam_visit ===
Sam: Thirty seven-year-olds and thirty recorders. I'm a hero. # thoughtful
-> END

=== sam_visit_lists ===
Sam: Benji makes lists. I make lists of Benji's lists. We're very happy. # happy
~ note(Keeps lists of Benji's lists.)
-> END

=== sam_visit_2_wedding ===
Sam: The wedding's on the beach if it's sunny and in the barn if it isn't. So, the barn. # happy
-> END

=== sam_request_wedding ===
# fits: quilt, sampler, cushion, wall_hanging
# best: sampler
Sam: I want to give Benji something for the wedding. Something with our names on, that we keep forever. # thoughtful
Sam: He'll cry. That's fine. That's the point.
-> END

=== sam_request_class ===
# fits: tote, plushie_sewn, plushie, scarf
# best: tote
Sam: I carry everything for school in a carrier bag. Sheet music, glue sticks, a recorder somebody bit. # sad
Sam: Something sturdier would make me look like a proper grown-up.
-> END

=== sam_handover_good ===
Sam: Oh, that's... yes. That's exactly right. Don't tell Benji I welled up. # happy
-> END

=== sam_handover_okay ===
Sam: Lovely. Thank you, really. # happy
-> END

=== sam_handover_poor ===
Sam: It's handmade. That counts for more than straight lines. # thoughtful
-> END

=== sam_suggest_yes ===
Sam: Yes. Let's do that. # happy
-> END

=== sam_suggest_buy ===
Sam: Could I buy the pattern? The kids could make it in art club. # happy
-> END

=== sam_suggest_no ===
Sam: It's lovely, but I'd only lose it under a pile of sheet music. # thoughtful
-> END

=== sam_scene_1 ===
Sam: Benji's terrified of the speech. He's written eleven drafts. They're all about wood. # happy
* [It'll be perfect because it's him.]
    Sam: That's what I said. He wrote a twelfth draft. # happy
    ~ friendship(10)
* [Help him cut it down?]
    Sam: You're braver than me. Bring snacks. # happy
    ~ friendship(10)
-> END

=== sam_scene_2 ===
Sam: Will you come to the wedding? You've made half of it. # happy
* [I wouldn't miss it.]
    Sam: Good. You're on Benji's side. He needs someone who won't cry before the vows. # happy
    ~ friendship(10)
* [Only if there's cake.]
    Sam: There are three cakes. Benji couldn't choose. # happy
    ~ friendship(10)
-> END

=== sam_scene_3 ===
Sam plays you the song they wrote for the wedding, on the ukulele, in the middle of your shop.
Sam: First audience. Be kind. # thoughtful
~ friendship(15)
-> END

=== sam_saturday ===
You spend Saturday helping Sam and Benji test wedding cake. Purely for research. A great deal of research. # happy
-> END

// ---- More lines (2026-11-02: so nobody repeats themselves), and how they take a present ----
=== sam_visit_choir ===
Sam: Thirty seven-year-olds singing "Sea Shanty No. 2" in four different keys. I've never been happier. # happy
-> END
=== sam_visit_benji ===
Sam: Benji's measuring things again. If he comes in and measures your counter, just let him. It helps. # thoughtful
-> END
=== sam_visit_3_nerves ===
Sam: Can I tell you something? I'm not nervous about the wedding. # thoughtful
Sam: I'm nervous about the first dance. I teach music. Everyone's going to expect me to be able to dance.
-> END
=== sam_saturday_piano ===
Sam plays you the song they're writing for the wedding, on the school hall's out-of-tune piano. It's lovely even with the sticky F. # happy
Sam: Don't tell Benji. It's a surprise. # happy
-> END
=== sam_gift ===
Sam: Oh, that's lovely. I'm going to hide it from Benji, or he'll build a cabinet just to keep it in. # happy
-> END
