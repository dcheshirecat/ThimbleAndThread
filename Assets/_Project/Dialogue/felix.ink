// Felix Adeyemi (he/him): runs Petals, the florist on Fore Street. Dreamy, talks to his plants, terrible with money.
// Friendship only. Chapter 2. Knot names follow Docs/ADDING_CONTENT.md, "Dialogue".

=== felix_intro ===
A man with pollen on his sleeves comes in carrying a bucket of tulips, apparently by accident. # surprised
Felix: Sorry, I came straight from a delivery. I'm Felix. Petals, the florist, on Fore Street. # happy
~ met()
~ note(Runs Petals, the florist on Fore Street.)
Felix: These are for you. Well. They were for the hotel. They can have the next bucket.
-> END

=== felix_visit ===
Felix: The ranunculus came in today. I may have cried a little. # happy
-> END

=== felix_visit_plants ===
Felix: I talk to the plants. Everyone says that, but I mean full conversations. The ferns are good listeners. # thoughtful
~ note(Talks to his plants. The ferns are the best listeners.)
-> END

=== felix_visit_2_money ===
Felix: I gave away more flowers than I sold this week. My accountant has stopped returning my calls. # sad
-> END

=== felix_request_shop ===
# fits: wall_hanging, cushion, tote, hoop_art
# best: wall_hanging
Felix: The shop wall behind the till is so empty. The flowers deserve a backdrop. # thoughtful
Felix: Something with texture. Something that looks like it grew there.
-> END

=== felix_request_bench ===
# fits: cushion, quilt, scarf
# best: cushion
Felix: I put a bench outside the shop so people can sit with the flowers. # happy
Felix: Nobody sits on it. It's too hard. It needs something soft.
-> END

=== felix_handover_good ===
Felix: Oh, it's perfect. It's like a garden you can keep. # happy
-> END

=== felix_handover_okay ===
Felix: Lovely. The tulips will be jealous. # happy
-> END

=== felix_handover_poor ===
Felix: It's got character. Like my wonkiest sunflower. I love that sunflower. # thoughtful
-> END

=== felix_suggest_yes ===
Felix: Yes! Make me one. I'll pay you in flowers. And money. Mostly money. # happy
-> END

=== felix_suggest_buy ===
Felix: Can I buy the pattern? I've been meaning to learn something with my hands that isn't secateurs. # happy
-> END

=== felix_suggest_no ===
Felix: Hmm. It's lovely, but it doesn't smell of anything. Not for me. # thoughtful
-> END

=== felix_scene_1 ===
Felix: Can I ask you something? How do you charge people for things you love making? # thoughtful
* [You charge what it's worth.]
    Felix: What it's worth. Right. I'll write that on my hand. # happy
    ~ friendship(10)
* [You don't, sometimes. And that's fine.]
    Felix: See, that's what I do, and the accountant hates it. # happy
    ~ friendship(10)
-> END

=== felix_scene_2 ===
Felix: My mum had a stall in Lagos. Flowers and fabric, side by side. I think that's why I came in here the first time. # thoughtful
~ note(His mum had a flower and fabric stall in Lagos.)
* [It smells right, doesn't it?]
    Felix: It does. Wool and lavender and something older. # happy
    ~ friendship(10)
* [Tell me about her.]
    Felix: She'd have liked you. She'd have tried to sell you a dahlia. # happy
    ~ friendship(10)
-> END

=== felix_scene_3 ===
Felix brings you a pot of lavender for the shop window.
Felix: It needs sun and someone to talk to. You'll manage both. # happy
~ friendship(15)
-> END

=== felix_saturday ===
You spend Saturday at the flower market with Felix, before dawn. He buys far too much and gives half of it away on the way home. # happy
-> END
