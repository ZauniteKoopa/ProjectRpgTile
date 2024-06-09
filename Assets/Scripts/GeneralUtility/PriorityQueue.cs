using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A priority queue that uses a min heap
public class PriorityQueue<T> where T : IComparable<T>
{
    //The mean heap as represented as an array
    private List<T> queue;

    //Constructor class
    public PriorityQueue()
    {
        queue = new List<T>();
    }

    //Adds an element to the priority queue: O(logN)
    public void Enqueue(T element)
    {
        //Add the element to the end of the list
        queue.Add(element);

        //Continuously swap with parent, if parent element is smaller: i - 1 = actual Index
        int i = queue.Count;
        while (i > 1 && queue[i - 1].CompareTo(queue[(i / 2) - 1]) < 0)
        {
            T temp = queue[i - 1];
            queue[i - 1] = queue[(i/2) - 1];
            queue[(i/2) - 1] = temp;

            i /= 2;
        }
    }

    //Removes an element from the priority queue. Always removes the top of the queue: O(logN)
    //  If queue is empty, returns null
    public T Dequeue()
    {
        if (queue.Count == 0)
            return default(T);
        
        //Get element to return.
        T top = queue[0];

        //Remove the last element and move it to the top
        queue[0] = queue[queue.Count - 1];
        queue.RemoveAt(queue.Count - 1);

        //Continously swap top element with children if children are smaller: i - 1 = actual Index in array
        int i = 1;
        bool digging = true;
        while (2 * i <= queue.Count && digging)
        {
            //Get the smallest of the 2 children (2 * i - 1 is left while 2 * i is right). If equal, just choose left
            int minChild = 2 * i - 1;

            if (2 * i < queue.Count && queue[minChild].CompareTo(queue[2 * i]) > 0)
                minChild = 2 * i;

            //Check if parent is bigger than smallest child to swap
            if (queue[minChild].CompareTo(queue[i - 1]) < 0)
            {
                T temp = queue[i - 1];
                queue[i - 1] = queue[minChild];
                queue[minChild] = temp;

                i = minChild + 1;
            }
            else
            {
                digging = false;
            }
        }

        //Return top most element
        return top;
    }

    //Method to look at the topmost element: O(1)
    //  if queue is empty, return default value
    public T Front()
    {
        if (queue.Count == 0)
            return default(T);
        return queue[0];
    }

    //Method to check if queue is empty O(1)
    public bool IsEmpty()
    {
        return queue.Count == 0;
    }

    // Method to clear the entire priority queue O(1)
    public void Clear() {
        queue.Clear();
    }


    // Function to update sort placement of an object, O(N) + O(logN)
    //  Pre: T MUST BE AN OBJECT AND NOT A PRIMITIVE, the updated object must be non-null and updated before hand. if not found within the list, do nothing
    //  Post: update the sort position of this object within the queue if found
    public void UpdateQueuePosition(T updated) {
        // Find the index where updated is found O(N)
        int curIndex = 0;
        while (curIndex < queue.Count && queue[curIndex].GetHashCode() != updated.GetHashCode()) {
            curIndex++;
        }

        // Only update the sort order if you found the element O(logN)
        if (curIndex < queue.Count) {
            // See if the element can be moved upwards
            bool goingUp = true;
            while (curIndex > 0 && goingUp) {
                // Get the upward index
                int upwardIndex = ((curIndex + 1) / 2) - 1;

                // If element at curIndex is smaller than element at upwardIndex swap
                goingUp = queue[curIndex].CompareTo(queue[upwardIndex]) < 0;
                if (goingUp) {
                    // Swap
                    T temp = queue[curIndex];
                    queue[curIndex] = queue[upwardIndex];
                    queue[upwardIndex] = temp;

                    // Change curIndex to upwardIndex
                    curIndex = upwardIndex;
                }
            }

            // See if element can be moved downwards
            bool goingDown = true;
            while ((curIndex + 1) * 2 <= queue.Count && goingDown) {
                // Get the index of the smallest of the 2 children
                int downwardIndex = ((curIndex + 1) * 2) - 1;
                downwardIndex = (downwardIndex + 1 < queue.Count && queue[downwardIndex + 1].CompareTo(queue[downwardIndex]) < 0) ?
                    downwardIndex + 1 : downwardIndex;
                
                // If element at curIndex is bigger than element at downwardIndex, swap down
                goingDown = queue[curIndex].CompareTo(queue[downwardIndex]) > 0;
                if (goingDown) {
                    // Swap
                    T temp = queue[curIndex];
                    queue[curIndex] = queue[downwardIndex];
                    queue[downwardIndex] = temp;

                    // Change curIndex to downwardIndex
                    curIndex = downwardIndex;
                }
            }
        }
    }

}
