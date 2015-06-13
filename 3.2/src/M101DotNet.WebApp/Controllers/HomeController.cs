﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using M101DotNet.WebApp.Models;
using M101DotNet.WebApp.Models.Home;
using MongoDB.Bson;
using System.Linq.Expressions;

namespace M101DotNet.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var blogContext = new BlogContext();
            // XXX WORK HERE
            // find the most recent 10 posts and order them
            // from newest to oldest
            var recentPosts = await blogContext.Posts.Find(new BsonDocument())
                .Sort(new BsonDocument("CreatedAtUtc", -1))
                .ToListAsync();

            var model = new IndexModel
            {
                RecentPosts = recentPosts
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult NewPost()
        {
            return View(new NewPostModel());
        }

        [HttpPost]
        public async Task<ActionResult> NewPost(NewPostModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var blogContext = new BlogContext();
            // XXX WORK HERE
            // Insert the post into the posts collection
            Post post = new Post();
            post.Author = User.Identity.Name;
            post.Title = model.Title;
            post.Content = model.Content;
            post.Tags = model.Tags.Split(',').ToList();
            post.CreatedAtUtc = DateTime.Now;
            post.Comments = new List<Comment>();

            await blogContext.Posts.InsertOneAsync(post);

            return RedirectToAction("Post", new { id = post.Id });
        }

        [HttpGet]
        public async Task<ActionResult> Post(string id)
        {
            var blogContext = new BlogContext();

            // XXX WORK HERE
            // Find the post with the given identifier
            var post = await blogContext.Posts.Find(p => p.Id == new ObjectId(id)).SingleOrDefaultAsync();

            if (post == null)
            {
                return RedirectToAction("Index");
            }

            var model = new PostModel
            {
                Post = post
            };

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Posts(string tag = null)
        {
            var blogContext = new BlogContext();

            // XXX WORK HERE
            // Find all the posts with the given tag if it exists.
            // Otherwise, return all the posts.
            // Each of these results should be in descending order.
            var filter = tag == null 
                ? new BsonDocument() 
                : Builders<Post>.Filter.AnyEq(x => x.Tags, tag).ToBsonDocument();
            var posts = await blogContext.Posts.Find(filter).ToListAsync();

            return View(posts);
        }

        [HttpPost]
        public async Task<ActionResult> NewComment(NewCommentModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Post", new { id = model.PostId });
            }

            var blogContext = new BlogContext();
            // XXX WORK HERE
            // add a comment to the post identified by model.PostId.
            // you can get the author from "this.User.Identity.Name"
            var post = await blogContext.Posts
                .Find(p => p.Id == new ObjectId(model.PostId)).SingleOrDefaultAsync();

            var comment = new Comment();
            comment.Author = this.User.Identity.Name;
            comment.Content = model.Content;
            comment.CreatedAtUtc = DateTime.Now;

            post.Comments.Add(comment);
            await blogContext.Posts
                .ReplaceOneAsync(new BsonDocument("_id", new ObjectId(model.PostId)), post);

            return RedirectToAction("Post", new { id = model.PostId });
        }
    }
}